using EnterpriseCollegeManagement.AcademicService.Data;
using EnterpriseCollegeManagement.AcademicService.Interfaces;

using EnterpriseCollegeManagement.AcademicService.Middleware;
using EnterpriseCollegeManagement.AcademicService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// LOG

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/AcademicService-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();


//dbcontext
builder.Services.AddDbContext<AcademicDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("AcademicDb")));
//httpclient(service to service communication)

builder.Services.AddHttpClient<IStudentServiceClient, StudentServiceClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:StudentServiceUrl"]!);
});



//DI

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<ICourseSubjectService, CourseSubjectService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IExamAttendanceService, ExamAttendanceService>();
builder.Services.AddScoped<IStudentAnswerService, StudentAnswerService>();
builder.Services.AddScoped<IStudentAnswerService, StudentAnswerService>();
builder.Services.AddScoped<IQuizGenerationService, QuizGenerationService>();
builder.Services.AddScoped<IAIQuizClient, GeminiQuizClient>(); //AI

builder.Services.AddHostedService<ExamAutoSubmitBackgroundService>();  //background service for autosubmit exams
//redis 
builder.Services.AddSingleton<IConnectionMultiplexer>(re =>

//IConnectionMultiplexer => this connection object tht allow our appplication to communicate with redis
//singletone => reuse redis connection rather than creating a new connection for every request.

{
    var connectionString = builder.Configuration["Redis:ConnectionString"];

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Redis connection string is missing.");
    }


    return ConnectionMultiplexer.Connect(connectionString!);

});

builder.Services.AddScoped<IRedisCacheService, RedisCacheService>(); 

builder.Services.AddDbContext<AcademicDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("AcademicDb")));

builder.Services.AddControllers();


// JWT Authentication

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwt = builder.Configuration.GetSection("Jwt");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Key"]!))
    };
});

builder.Services.AddAuthorization();






// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Swagger

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter JWT token"
        });

    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference =
                        new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});





var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseMiddleware<GlobalExceptionMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
