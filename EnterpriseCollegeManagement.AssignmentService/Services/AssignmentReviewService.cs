using EnterpriseCollegeManagement.AssignmentService.Data;
using EnterpriseCollegeManagement.AssignmentService.DTOs.Requests;
using EnterpriseCollegeManagement.AssignmentService.DTOs.Responses;
using EnterpriseCollegeManagement.AssignmentService.Helpers;
using EnterpriseCollegeManagement.AssignmentService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCollegeManagement.AssignmentService.Services
{
    public class AssignmentReviewService : IAssignmentReviewService
    {
        private readonly AssignmentDbContext _context;
        private readonly PdfTextExtractor _pdfTextExtractor;
        private readonly IAssignmentReviewClient _assignmentReviewClient;
        private readonly ILogger<AssignmentReviewService> _logger;

        public AssignmentReviewService(AssignmentDbContext context,
                                       PdfTextExtractor pdfTextExtractor,
                                       IAssignmentReviewClient assignmentReviewClient,
                                       ILogger<AssignmentReviewService> logger)
        {

            _context = context;
            _assignmentReviewClient = assignmentReviewClient;
            _pdfTextExtractor = pdfTextExtractor;
            _logger = logger;

        }


        public async Task<AIReviewResponseDto> ReviewAssignmentAsync(int assignmentId, int submissionId, string currentUserId, bool isAdmin)
        {
            _logger.LogInformation("AI assignment review started. AssignmentId: {AssignmentId}, SubmissionId: {SubmissionId}, UserId: {UserId}",
               assignmentId, submissionId, currentUserId);

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                _logger.LogWarning("AI review rejected because the user ID is empty.");
                throw new ArgumentException("User ID is required.", nameof(currentUserId));

            }

            var assignment  = await _context.Assignments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == assignmentId && !x.IsDeleted);
            if(assignment ==  null)
            {
                _logger.LogWarning("Assignment not found for AI review. AssignmentId: {AssignmentId}", assignmentId);

                throw new KeyNotFoundException("Assignment not found.");
            }

            var submission = await _context.AssignmentSubmissions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == submissionId && x.AssignmentId == assignmentId && !x.IsDeleted);

            if(submission == null)
            {
                _logger.LogWarning("Submission not found for AI review. AssignmentId: {AssignmentId}, SubmissionId: {SubmissionId}",assignmentId, submissionId);

                throw new KeyNotFoundException("Submission not found for this assignment.");
            }

            if(!isAdmin && assignment.TeacherUserId != currentUserId)
            {
                _logger.LogWarning("Unauthorized AI review attempt. UserId: {UserId}, AssignmentId: {AssignmentId}",currentUserId, assignmentId);

                throw new UnauthorizedAccessException("You are not authorized to review this assignment.");
            }

            var contentParts = new List<string>(); //for submission text 

            if (!string.IsNullOrWhiteSpace(submission.SubmissionText))
            {
                contentParts.Add(submission.SubmissionText);
            }

            if (!string.IsNullOrWhiteSpace(submission.FileUrl))
            {
                _logger.LogInformation("Extracting PDF text for submission. SubmissionId: {SubmissionId}", submissionId);

                var pdfText = await _pdfTextExtractor.ExtractTextAsync(submission.FileUrl); //passing file url to the function

                if (!string.IsNullOrWhiteSpace(pdfText))
                {
                    contentParts.Add(pdfText);
                }

            }

            var submissionContent = string.Join(Environment.NewLine + Environment.NewLine, contentParts); //make line break b/w each added content 

            if (string.IsNullOrWhiteSpace(submissionContent))
            {
                _logger.LogWarning("No submission content available for AI review. SubmissionId: {SubmissionId}", submissionId);

                throw new InvalidOperationException("No readable submission content is available for AI review.");
            }

            //context object for gemini 

            var reviewContext = new AssignmentReviewContextDto
            {
                AssignmentId = assignment.Id,
                SubmissionId = submission.Id,
                AssignmentTitle = assignment.Title,
                AssignmentDescription = assignment.Description,
                MaxMarks = assignment.MaxMarks,
                SubmissionContent = submissionContent
            };

            _logger.LogInformation("Assignment review context prepared. AssignmentId: {AssignmentId}, SubmissionId: {SubmissionId}, ContentLength: {ContentLength}",
             assignmentId, submissionId, submissionContent.Length);

            //calling the function and passing Dto  Ai 
            var review = await _assignmentReviewClient.ReviewAssignmentAsync(reviewContext);

            if(review.SuggestedMarks > assignment.MaxMarks || review.SuggestedMarks < 0)
            {
                _logger.LogWarning("AI returned invalid suggested marks. SubmissionId: {SubmissionId}, SuggestedMarks: {SuggestedMarks}, MaxMarks: {MaxMarks}",
                    submissionId, review.SuggestedMarks, assignment.MaxMarks);

                throw new InvalidOperationException("AI returned suggested marks outside the allowed range.");
            }
            _logger.LogInformation("AI assignment review completed. AssignmentId: {AssignmentId}, SubmissionId: {SubmissionId}, SuggestedMarks: {SuggestedMarks}",
               assignmentId, submissionId, review.SuggestedMarks);

           
            return review; //suggestions from ai for teacher review 
        }

      
       
    }
}
