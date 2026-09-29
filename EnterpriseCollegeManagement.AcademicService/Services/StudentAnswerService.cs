using EnterpriseCollegeManagement.AcademicService.Data;
using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;
using EnterpriseCollegeManagement.AcademicService.Entities;
using EnterpriseCollegeManagement.AcademicService.Exceptions;
using EnterpriseCollegeManagement.AcademicService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCollegeManagement.AcademicService.Services
{
    public class StudentAnswerService : IStudentAnswerService
    {
        private readonly AcademicDbContext _context;
        private readonly ILogger<StudentAnswerService> _logger;
        private readonly IStudentServiceClient _studentServiceClient;

        public StudentAnswerService(AcademicDbContext context, ILogger<StudentAnswerService> logger, IStudentServiceClient studentServiceClient)
        {
            _context = context;
            _logger = logger;
            _studentServiceClient = studentServiceClient;
        }


        public async Task<StudentAnswerResponseDto> SaveAnswerAsync(CreateStudentAnswerRequestDto request, string studentUserId)
        {
            _logger.LogInformation("Saving student answer. AttendanceId: {AttendanceId}, QuestionId: {QuestionId}, StudentUserId: {StudentUserId}",
                request.ExamAttendanceId,
                request.QuestionId,
                studentUserId);

            var selectedoption = request.SelectedOption.Trim().ToUpper();

            if(selectedoption !=  "A" && selectedoption != "B" && selectedoption != "C" && selectedoption != "D")
            {
                _logger.LogWarning("Invalid selected option. AttendanceId: {AttendanceId}, QuestionId: {QuestionId}, SelectedOption: {SelectedOption}",
                   request.ExamAttendanceId,
                   request.QuestionId,
                   request.SelectedOption);

                throw new BadRequestException("Selected option must be A, B, C or D.");
            }

            var attendance = await _context.ExamAttendances.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ExamAttendanceId);
            if(attendance == null)
            {
                _logger.LogWarning("Exam attendance not found. AttendanceId: {AttendanceId}",request.ExamAttendanceId);

                throw new NotFoundException("Exam attendance not found.");
            }

            if(attendance.StudentUserId != studentUserId)
            {
                _logger.LogWarning("Student attempted to use another student's attendance. AttendanceId: {AttendanceId}, StudentUserId: {StudentUserId}",
                  request.ExamAttendanceId,
                  studentUserId);

                throw new BadRequestException("This exam attendance does not belong to the current student.");
            }

            if (attendance.IsSubmitted)
            {
                _logger.LogWarning( "Student attempted to answer after exam submission. AttendanceId: {AttendanceId}", request.ExamAttendanceId);

                throw new BadRequestException("Exam has already been submitted.");
            }

            var examExists = await _context.Exams.AsNoTracking().AnyAsync(x => x.Id == attendance.ExamId && !x.IsDeleted);

            if(!examExists)
            {
                _logger.LogWarning("Exam not found. ExamId: {ExamId}", attendance.ExamId);

                throw new NotFoundException("Exam not found.");
            }

            var question = await _context.Questions.AsNoTracking().FirstOrDefaultAsync(x => x.Id==request.QuestionId);
            if(question == null)
            {
                _logger.LogWarning("Question not found. QuestionId: {QuestionId}",request.QuestionId);

                throw new NotFoundException("Question not found.");
            }

            if(question.ExamId != attendance.ExamId)
            {
                _logger.LogWarning("Question does not belong to attendance exam. QuestionId: {QuestionId}, QuestionExamId: {QuestionExamId}, AttendanceExamId: {AttendanceExamId}",
                  request.QuestionId,
                  question.ExamId,
                  attendance.ExamId);

                throw new BadRequestException("Question does not belong to this exam.");
            }

            var alreadyAnswered = await _context.StudentAnswers.AsNoTracking()
                .AnyAsync(x => x.ExamAttendanceId == request.ExamAttendanceId && x.QuestionId == request.QuestionId);
            if(alreadyAnswered)
            {
                _logger.LogWarning( "Question has already been answered. AttendanceId: {AttendanceId}, QuestionId: {QuestionId}",
                   request.ExamAttendanceId,
                   request.QuestionId);

                throw new ConflictException("This question has already been answered.");
            }

            //all questions

            var questions = await _context.Questions.AsNoTracking()
                .Where(x => x.ExamId == attendance.ExamId && !x.IsDeleted)
                .OrderBy(x => x.CreatedDate).ThenBy(x => x.Id).ToListAsync();

            var answeredQuestionsId = await _context.StudentAnswers.AsNoTracking()
                .Where(x => x.ExamAttendanceId == request.ExamAttendanceId)
                .Select(x => x.Id).ToListAsync(); //list of id 

            var nextunanswerQuestion = questions.FirstOrDefault(x => !answeredQuestionsId.Contains(x.Id));

            if (nextunanswerQuestion == null)
            {
                _logger.LogWarning( "All questions have already been answered. AttendanceId: {AttendanceId}", request.ExamAttendanceId);

                throw new BadRequestException( "All questions have already been answered.");
            }

            if(nextunanswerQuestion.Id != request.QuestionId)
            {
                _logger.LogWarning("Student attempted to answer question out of order. ExpectedQuestionId: {ExpectedQuestionId}, RequestedQuestionId: {RequestedQuestionId}, AttendanceId: {AttendanceId}",
                  nextunanswerQuestion.Id,
                  request.QuestionId,
                  request.ExamAttendanceId);

                throw new BadRequestException( $"Please answer question {nextunanswerQuestion.Id} first.");
            }

            var correctOption = question.CorrectOption.Trim().ToUpper();

            var iscorrect = correctOption == selectedoption;

            var mark = iscorrect ? question.Marks : 0;

            var studentAnswer = new StudentAnswer
            {
                ExamAttendanceId = request.ExamAttendanceId,
                QuestionId = request.QuestionId,
                SelectedOption = selectedoption,
                IsCorrect = iscorrect,
                MarksAwarded = mark
            };

            _context.StudentAnswers.Add(studentAnswer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Student answer saved successfully. AnswerId: {AnswerId}, AttendanceId: {AttendanceId}, QuestionId: {QuestionId}, IsCorrect: {IsCorrect}, MarksAwarded: {MarksAwarded}",
               studentAnswer.Id,
               studentAnswer.ExamAttendanceId,
               studentAnswer.QuestionId,
               studentAnswer.IsCorrect,
               studentAnswer.MarksAwarded);

            return new StudentAnswerResponseDto
            {
                Id = studentAnswer.Id,
                ExamAttendanceId = studentAnswer.ExamAttendanceId,
                QuestionId = studentAnswer.QuestionId,
                SelectedOption = studentAnswer.SelectedOption,
                IsCorrect = studentAnswer.IsCorrect,
                MarksAwarded = studentAnswer.MarksAwarded
            };
        }
    }
}
