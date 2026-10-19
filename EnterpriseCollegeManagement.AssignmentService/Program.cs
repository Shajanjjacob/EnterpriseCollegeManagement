using EnterpriseCollegeManagement.AssignmentService.Data;
using EnterpriseCollegeManagement.AssignmentService.Helpers;
using EnterpriseCollegeManagement.AssignmentService.Interfaces;
using EnterpriseCollegeManagement.AssignmentService.Middleware;
using EnterpriseCollegeManagement.AssignmentService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// LOG

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/AssignmentService-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();



//dbcontext

builder.Services.AddDbContext<AssignmentDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("AssignmentDb")));

//Service - Service

builder.Services.AddHttpClient<ITeacherServiceClient, TeacherCourseSubjectService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:TeacherService"]
         ?? throw new InvalidOperationException("TeacherService URL is not configured."));
});

builder.Services.AddHttpClient<IAcademicServiceClient, AcademicServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:AcademicService"]
        ?? throw new InvalidOperationException("AcademicService URL is not configured."));
});

builder.Services.AddHttpClient<IAuditServiceClient, AuditServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:IdentityService"]
        ?? throw new InvalidOperationException("AuditService URL is not configured."));
});

builder.Services.AddHttpClient<IStudentServiceClient, StudentServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:StudentService"]
        ?? throw new InvalidOperationException("StudentService URL is not configured."));
});


//DI
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IAssignmentSubmissionService , AssignmentSubmissionService>();

builder.Services.AddScoped<PdfTextExtractor>();//pdf file text extractor 

builder.Services.AddScoped<IAssignmentReviewService, AssignmentReviewService>();

builder.Services.AddScoped<IAssignmentReviewClient, GeminiAssignmentReviewClient>();

builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>(); //local file storage

//JWT
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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

// Swagger

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddServer(
    new Microsoft.OpenApi.Models.OpenApiServer
    {
        Url = "/assignments"  // api gateway api/assignment/1 => /assignments/api/assignment/1
    });
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


//health checkup 

builder.Services.AddHealthChecks();


var app = builder.Build();

//middleware

app.UseMiddleware<GlobalExceptionMiddleware>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.MapHealthChecks("/health"); //health

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
