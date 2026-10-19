
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Forwarder;

var builder = WebApplication.CreateBuilder(args);


//SERILOG CONFIGURATION


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/ApiGateway-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();


//CONTROLLERS


builder.Services.AddControllers();

//SWAGGER CONFIGURATION


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Enterprise College Management API Gateway",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// JWT AUTHENTICATION


var jwt = builder.Configuration.GetSection("Jwt");

var issuer = jwt["Issuer"];
var audience = jwt["Audience"];
var key = jwt["Key"];

if (string.IsNullOrWhiteSpace(issuer) ||
    string.IsNullOrWhiteSpace(audience) ||
    string.IsNullOrWhiteSpace(key))
{
    throw new InvalidOperationException(
        "JWT configuration is missing. Check Jwt:Issuer, Jwt:Audience, and Jwt:Key in User Secrets.");
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)),

            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });


// AUTHORIZATION


builder.Services.AddAuthorization();

//Rate Limiting

builder.Services.AddRateLimiter(option =>
{
    option.GlobalLimiter =
       PartitionedRateLimiter.Create<HttpContext, string>(
           httpContext =>
               RateLimitPartition.GetFixedWindowLimiter(
                   partitionKey: "gateway-global",
                   factory: _ => new FixedWindowRateLimiterOptions
                   {
                       PermitLimit = 5,
                       Window = TimeSpan.FromSeconds(30), //30s 5 req
                       AutoReplenishment = true,
                       QueueLimit = 0
                   }));

    option.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;



});







// YARP REVERSE PROXY


builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

//health check => A health check tells us whether an application or service is running and responding.
var healthChecks = builder.Services.AddHealthChecks();


healthChecks.AddUrlGroup(
    new Uri(builder.Configuration["HealthChecks:IdentityService"]!),
    name: "identity-service");

healthChecks.AddUrlGroup(
    new Uri(builder.Configuration["HealthChecks:StudentService"]!),
    name: "student-service");

healthChecks.AddUrlGroup(
    new Uri(builder.Configuration["HealthChecks:AcademicService"]!),
    name: "academic-service");

healthChecks.AddUrlGroup(
    new Uri(builder.Configuration["HealthChecks:TeacherService"]!),
    name: "teacher-service");

healthChecks.AddUrlGroup(
    new Uri(builder.Configuration["HealthChecks:AssignmentService"]!),
    name: "assignment-service");


builder.Services.AddRequestTimeouts();

//CORS

var frontendUrl = builder.Configuration["Frontend:BaseUrl"];

if (string.IsNullOrWhiteSpace(frontendUrl))
{
    throw new InvalidOperationException( "Frontend:BaseUrl is missing. Configure it in User Secrets or appsettings.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(frontendUrl!)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});



var app = builder.Build();




app.MapHealthChecks("/health"); //using /health in  api gateways url we can check (get a message "Healthy")

//SWAGGER MIDDLEWARE


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();


    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "API Gateway v1");

        options.SwaggerEndpoint(
            "/students/swagger/v1/swagger.json",
            "StudentService v1");

        options.SwaggerEndpoint(
            "/identity/swagger/v1/swagger.json",
            "IdentityService v1");

        options.SwaggerEndpoint(
       "/academic/swagger/v1/swagger.json",
       "AcademicService v1");

        options.SwaggerEndpoint(
        "/teachers/swagger/v1/swagger.json",
        "TeacherService v1");

        options.SwaggerEndpoint(
    "/assignments/swagger/v1/swagger.json",
    "AssignmentService v1");

        options.RoutePrefix = "swagger";
    });

}



app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseRouting();

app.UseRequestTimeouts(); // Apply YARP route timeouts

app.UseCors("FrontendPolicy"); //CORS

app.UseRateLimiter(); //RATE LIMIT

app.UseAuthentication();

app.UseAuthorization();



app.MapControllers(); //

app.MapReverseProxy();

//logg

try
{
    Log.Information("API Gateway starting");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
