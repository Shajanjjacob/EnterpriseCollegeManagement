using EnterpriseCollegeManagement.StudentService.Data;
using EnterpriseCollegeManagement.StudentService.Interfaces;
using EnterpriseCollegeManagement.StudentService.Mapping;
using EnterpriseCollegeManagement.StudentService.Middleware;
using EnterpriseCollegeManagement.StudentService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//DI

builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddDbContext<StudentDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DbContext")));

//automapper

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

//httpclient(data get b/w diff services (service-to-service communication))

builder.Services.AddHttpClient<IIdentityServiceClient, IdentityServiceClient>(
    client =>
    {
        client.BaseAddress = new Uri("https://localhost:72/");
    });


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




//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
