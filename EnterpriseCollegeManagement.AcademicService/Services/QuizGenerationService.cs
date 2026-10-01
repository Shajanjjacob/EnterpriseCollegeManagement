using Azure;
using EnterpriseCollegeManagement.AcademicService.Data;
using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;
using EnterpriseCollegeManagement.AcademicService.Entities;
using EnterpriseCollegeManagement.AcademicService.Exceptions;
using EnterpriseCollegeManagement.AcademicService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCollegeManagement.AcademicService.Services
{
    public class QuizGenerationService : IQuizGenerationService
    {
        private readonly AcademicDbContext _context;
        private readonly ILogger<IQuizGenerationService> _logger;

        private readonly IAIQuizClient _aIQuizClient; //

        public QuizGenerationService(AcademicDbContext context, ILogger<IQuizGenerationService> logger, IAIQuizClient aIQuizClient)
        {
            _context = context;
            _logger = logger;
            _aIQuizClient = aIQuizClient;
        }

        public async Task<GenerateQuizResponseDto> GenerateQuizAsync(GenerateQuizRequestDto request, string actorUserId)
        {
            _logger.LogInformation("AI quiz generation started. ExamId: {ExamId}, Topic: {Topic}, NumberOfQuestions: {NumberOfQuestions}, ActorUserId: {ActorUserId}",
             request.ExamId,
             request.Topic,
             request.NumberOfQuestions,
             actorUserId);

            if (request.NumberOfQuestions <= 0)
            {
                _logger.LogWarning("AI quiz generation failed. Invalid question count: {NumberOfQuestions}", request.NumberOfQuestions);

                throw new BadRequestException("Number of questions must be greater than zero.");
            }

            var exam = await _context.Exams.AsNoTracking()
                .Include(x => x.courseSubject)
                .ThenInclude(x => x.Subject)
                .Include(x => x.courseSubject)
                .ThenInclude(x => x.Course)
                .FirstOrDefaultAsync(x => x.Id == request.ExamId && !x.IsDeleted);
            if (exam == null)
            {
                _logger.LogWarning("AI quiz generation failed. Exam not found. ExamId: {ExamId}", request.ExamId);

                throw new NotFoundException("Exam not found.");
            }

            if (exam.IsPublished) //particualr xam alrdy published bring error 
            {
                _logger.LogWarning("AI quiz generation failed. Exam is already published. ExamId: {ExamId}", exam.Id);

                throw new BadRequestException("Cannot generate quiz because the exam is already published.");
            }

            if (string.IsNullOrWhiteSpace(request.Topic))
            {
                _logger.LogWarning("AI quiz generation failed. Topic is empty. ExamId: {ExamId}", request.ExamId);

                throw new BadRequestException("Topic is required.");
            }

            if (exam.TotalMarks <= 0)
            {
                _logger.LogWarning("AI quiz generation failed. Invalid exam total marks. ExamId: {ExamId}, TotalMarks: {TotalMarks}", exam.Id, exam.TotalMarks);

                throw new BadRequestException("Exam total marks must be greater than zero.");
            }

            if (exam.TotalMarks % request.NumberOfQuestions != 0)
            {
                _logger.LogWarning("AI quiz generation failed. Total marks cannot be evenly divided. ExamId: {ExamId}, TotalMarks: {TotalMarks}, NumberOfQuestions: {NumberOfQuestions}",
                   exam.Id,
                   exam.TotalMarks,
                   request.NumberOfQuestions);

                throw new BadRequestException("Exam total marks must be evenly divisible by the number of questions.");
            }
            var markPerQuestions = exam.TotalMarks / request.NumberOfQuestions;

            var examContext = new QuizGenerationContextDto
            {
                ExamId = exam.Id,
                ExamTitle = exam.Title,
                ExamDescription = exam.Description,
                CourseName = exam.courseSubject.Course.Name,
                SubjectName = exam.courseSubject.Subject.Name,
                Semester = exam.courseSubject.Semester,
                Topic = request.Topic.Trim(),
                Notes = request.Notes?.Trim(),
                NumberOfQuestions = request.NumberOfQuestions,
                MarksPerQuestion = markPerQuestions
            };

            _logger.LogInformation("AI quiz context prepared. ExamId: {ExamId}, Course: {Course}, Subject: {Subject}, Semester: {Semester}, MarksPerQuestion: {MarksPerQuestion}",
              exam.Id, exam.courseSubject.Course.Name, exam.courseSubject.Subject.Name,
              exam.courseSubject.Semester,
              markPerQuestions);

            //////

            var generatedQuestions = await _aIQuizClient.GenerateQuestionsAsync(examContext); //calling gemini 

            /////
            if (generatedQuestions == null || generatedQuestions.Count == 0)
            {
                throw new BadRequestException("AI did not generate any questions.");
            }

            if (generatedQuestions.Count != request.NumberOfQuestions)
            {
                throw new BadRequestException($"AI generated {generatedQuestions.Count} questions, " + $"but {request.NumberOfQuestions} were requested.");
            }

            var questionEntities = new List<Question>();

            foreach (var generatedQuestion in generatedQuestions)
            {
                if (string.IsNullOrWhiteSpace(generatedQuestion.QuestionText) || string.IsNullOrWhiteSpace(generatedQuestion.OptionA) || string.IsNullOrWhiteSpace(generatedQuestion.OptionB) || string.IsNullOrWhiteSpace(generatedQuestion.OptionC) || string.IsNullOrWhiteSpace(generatedQuestion.OptionD))
                {
                    throw new BadRequestException("AI generated an incomplete question.");
                }

                var correctOption = generatedQuestion.CorrectOption.Trim().ToUpper();


                if (correctOption != "A" && correctOption != "B" && correctOption != "C" && correctOption != "D")
                {
                    throw new BadRequestException("AI generated an invalid correct option.");
                }

                var question = new Question
                {
                    ExamId = exam.Id,
                    QuestionText = generatedQuestion.QuestionText,
                    OptionA = generatedQuestion.OptionA,
                    OptionB = generatedQuestion.OptionB,
                    OptionC = generatedQuestion.OptionC,
                    OptionD = generatedQuestion.OptionD,
                    CorrectOption = correctOption,
                    Marks = markPerQuestions,
                    CreatedBy = actorUserId,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };
                questionEntities.Add(question);

               
            }

            await _context.Questions.AddRangeAsync(questionEntities);  // each time loop object get added 

            await _context.SaveChangesAsync();

            foreach (var generatedQuestion in generatedQuestions)
            {
                generatedQuestion.Marks = markPerQuestions;
            }

            _logger.LogInformation("AI quiz generated successfully. ExamId: {ExamId}, Questions: {QuestionCount}", exam.Id, questionEntities.Count);
            var response = new GenerateQuizResponseDto
            {
                ExamId = exam.Id,
                Questions = generatedQuestions
            };
            return response;
        }
    }
}

