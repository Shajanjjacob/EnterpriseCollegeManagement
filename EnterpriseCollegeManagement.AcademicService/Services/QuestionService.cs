using EnterpriseCollegeManagement.AcademicService.Data;
using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;
using EnterpriseCollegeManagement.AcademicService.Entities;
using EnterpriseCollegeManagement.AcademicService.Exceptions;
using EnterpriseCollegeManagement.AcademicService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCollegeManagement.AcademicService.Services
{
    public class QuestionService : IQuestionService
    {

        private readonly AcademicDbContext _context;
        private readonly ILogger<QuestionService> _logger;
        public QuestionService(AcademicDbContext context, ILogger<QuestionService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<QuestionResponseDto> CreateQuestionAsync(CreateQuestionRequestDto request, string actorUserId)
        {
            _logger.LogInformation( "Creating question. ExamId: {ExamId}, ActorUserId: {ActorUserId}",request.ExamId,actorUserId);

            if(request.Marks <= 0)
            {
                _logger.LogWarning( "Question creation failed. Invalid marks: {Marks}",request.Marks);

                throw new BadRequestException("Marks must be greater than zero.");
            }

            var exam = await _context.Exams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ExamId && !x.IsDeleted);
            if(exam == null)
            {
                _logger.LogWarning( "Question creation failed. Exam not found. ExamId: {ExamId}",request.ExamId);

                throw new NotFoundException( "Exam not found.");
            }

            var question = new Question
            {
                ExamId = request.ExamId,
                QuestionText = request.QuestionText.Trim(),
                OptionA = request.OptionA.Trim(),
                OptionB = request.OptionB.Trim(),
                OptionC = request.OptionC.Trim(),
                OptionD = request.OptionD.Trim(),
                CorrectOption = request.CorrectOption.Trim(),
                Marks = request.Marks,

                CreatedBy = actorUserId,
                CreatedDate = DateTime.UtcNow,

                IsDeleted = false
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Question created successfully. QuestionId: {QuestionId}, ExamId: {ExamId}",question.Id,question.ExamId);

            return new QuestionResponseDto
            {
                Id = question.Id,
                ExamId = question.ExamId,
                QuestionText = question.QuestionText,
                OptionA = question.OptionA,
                OptionB = question.OptionB,
                OptionC = question.OptionC,
                OptionD = question.OptionD,
                CorrectOption = question.CorrectOption,
                Marks = question.Marks,
                CreatedDate = question.CreatedDate,

            };
        }

        public async Task<bool> DeleteQuestionAsync(int id, string actorUserId)
        {
            _logger.LogInformation( "Deleting question. QuestionId: {QuestionId}, ActorUserId: {ActorUserId}", id, actorUserId);
            var question = await _context.Questions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if(question == null)
            {
                _logger.LogWarning( "Question deletion failed. Question not found. QuestionId: {QuestionId}",id);

                throw new NotFoundException("Question not found.");

            }

            question.DeletedBy = actorUserId;
            question.DeletedDate = DateTime.UtcNow;
            question.IsDeleted = true;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Question soft deleted successfully. QuestionId: {QuestionId}",question.Id);

            return true;
        }

        public async Task<QuestionResponseDto?> GetQuestionByIdAsync(int id)
        {
            _logger.LogInformation( "Getting question by ID. QuestionId: {QuestionId}", id);

            var question = await _context.Questions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (question == null)
            {
                _logger.LogWarning( "Question not found. QuestionId: {QuestionId}",id);

                return null;
            }

            return new QuestionResponseDto
            {
                Id = question.Id,
                ExamId = question.ExamId,
                QuestionText = question.QuestionText,
                OptionA = question.OptionA,
                OptionB = question.OptionB,
                OptionC = question.OptionC,
                OptionD = question.OptionD,
                CorrectOption = question.CorrectOption,
                Marks = question.Marks,
                CreatedDate = question.CreatedDate,
                UpdatedDate = question.UpdatedDate
            };
        }

        public async Task<List<QuestionResponseDto>> GetQuestionsByExamIdAsync(int examId)
        {
            _logger.LogInformation("Getting questions by ExamId. ExamId: {ExamId}", examId);

            var examExists = await _context.Exams.AsNoTracking().AnyAsync(x =>  x.Id == examId && !x.IsDeleted);

            if (!examExists)
            {
                _logger.LogWarning( "Questions retrieval failed. Exam not found. ExamId: {ExamId}", examId);

                throw new NotFoundException("Exam not found.");
            }


            var questions =  _context.Questions.AsNoTracking().Where(x=> x.ExamId == examId && !x.IsDeleted).OrderBy(x => x.Id).ToList();

            _logger.LogInformation("Retrieved {QuestionCount} questions for ExamId: {ExamId}", questions.Count,examId);

            return questions.Select(x => new QuestionResponseDto
            {
                Id = x.Id,
                ExamId = x.ExamId,
                QuestionText = x.QuestionText,
                OptionA = x.OptionA,
                OptionB = x.OptionB,
                OptionC = x.OptionC,
                OptionD = x.OptionD,
                CorrectOption = x.CorrectOption,
                Marks = x.Marks,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate


            }).ToList();
        }

        public async Task<QuestionResponseDto> UpdateQuestionAsync(int id, CreateQuestionRequestDto request, string actorUserId)
        {
            _logger.LogInformation( "Updating question. QuestionId: {QuestionId}, ActorUserId: {ActorUserId}",id,actorUserId);

            if (request.Marks <= 0)
            {
                _logger.LogWarning( "Question update failed. Invalid marks: {Marks}",request.Marks);

                throw new BadRequestException("Marks must be greater than zero.");
            }


            var examExists = await _context.Exams.AsNoTracking().AnyAsync(x => x.Id == request.ExamId && !x.IsDeleted);
            if(!examExists)
            {
                _logger.LogWarning( "Question update failed. Exam not found. ExamId: {ExamId}",request.ExamId);

                throw new NotFoundException("Exam not found.");
            }

            var question = await _context.Questions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if(question == null)
            {
                _logger.LogWarning( "Question update failed. Question not found. QuestionId: {QuestionId}",id);

                throw new NotFoundException("Question not found.");

            }

            question.ExamId = request.ExamId;
            question.QuestionText = request.QuestionText.Trim();
            question.OptionA = request.OptionA.Trim();
            question.OptionB = request.OptionB.Trim();
            question.OptionC = request.OptionC.Trim();
            question.OptionD = request.OptionD.Trim();
            question.CorrectOption = request.CorrectOption.Trim();
            question.Marks = request.Marks;

            question.UpdatedBy = actorUserId;
            question.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation( "Question updated successfully. QuestionId: {QuestionId}", question.Id);

            return new QuestionResponseDto
            {
                Id = question.Id,
                ExamId = question.ExamId,
                QuestionText = question.QuestionText,
                OptionA = question.OptionA,
                OptionB = question.OptionB,
                OptionC = question.OptionC,
                OptionD = question.OptionD,
                CorrectOption = question.CorrectOption,
                Marks = question.Marks,
                CreatedDate = question.CreatedDate,
                UpdatedDate = question.UpdatedDate
            };


        }
    }
}
