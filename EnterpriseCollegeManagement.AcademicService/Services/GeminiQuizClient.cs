using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;
using EnterpriseCollegeManagement.AcademicService.Interfaces;
using Google.GenAI;
using Google.GenAI.Types;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EnterpriseCollegeManagement.AcademicService.Services
{
    public class GeminiQuizClient : IAIQuizClient
    {
        private readonly Client _client; // useing client we used to req to gemini
        private readonly string _model;


        public GeminiQuizClient(IConfiguration configuration)
        {
            var apiKey = configuration["AI:Gemini:ApiKey"];

            _model = configuration["AI:Gemini:Model"]
                     ?? "gemini-3.5-flash-lite";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Gemini API key is not configured.");
            }

            _client = new Client(apiKey: apiKey);

        }

        public async Task<List<GeneratedQuestionDto>> GenerateQuestionsAsync(QuizGenerationContextDto context)
        {

            var prompt = BuildPrompt(context);

            var schemaString = """
            {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "questionText": {
                    "type": "string"
                  },
                  "optionA": {
                    "type": "string"
                  },
                  "optionB": {
                    "type": "string"
                  },
                  "optionC": {
                    "type": "string"
                  },
                  "optionD": {
                    "type": "string"
                  },
                  "correctOption": {
                    "type": "string",
                    "enum": ["A", "B", "C", "D"]
                  }
                },
                "required": [
                  "questionText",
                  "optionA",
                  "optionB",
                  "optionC",
                  "optionD",
                  "correctOption"
                ]
              }
            }
            """;

            var config = new GenerateContentConfig
            {
                ResponseMimeType = "application/json",
                ResponseJsonSchema = JsonNode.Parse(schemaString),
                Temperature = 0.2
            };

            var response = await _client.Models.GenerateContentAsync(
                model: _model,
                contents: prompt,
                config: config);

            var jsonText = response.Candidates?
                .FirstOrDefault()?
                .Content?
                .Parts?
                .FirstOrDefault()?
                .Text;

            if (string.IsNullOrWhiteSpace(jsonText))
            {
                throw new InvalidOperationException(
                    "Gemini returned an empty response.");
            }

            var questions =  JsonSerializer.Deserialize<List<GeneratedQuestionDto>>(jsonText,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (questions == null || questions.Count == 0)
            {
                throw new InvalidOperationException(
                    "Gemini did not generate any questions.");
            }

            return questions;   //list of gen qustns 
        }

        private static string BuildPrompt( QuizGenerationContextDto context)
             {
                            return $"""
                You are an educational multiple-choice question generator.

                Generate exactly {context.NumberOfQuestions} multiple-choice questions.

                Exam:{context.ExamTitle}

                Exam Description:{context.ExamDescription}

                Course: {context.CourseName}

                Subject:{context.SubjectName}

                Semester:{context.Semester}

                Topic: {context.Topic}

                Study Notes: {context.Notes}

                Rules:
                - Generate exactly {context.NumberOfQuestions} questions.
                - Every question must have exactly four options.
                - Use options A, B, C and D.
                - Exactly one option must be correct.
                - correctOption must be only A, B, C or D.
                - Questions must be relevant to the subject and topic.
                - Do not create duplicate questions.
                - Do not include explanations.
                - Do not include marks.
                - Do not include markdown.
                """;

        }
    }
}
