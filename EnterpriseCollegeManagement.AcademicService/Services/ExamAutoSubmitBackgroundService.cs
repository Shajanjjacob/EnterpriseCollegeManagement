using EnterpriseCollegeManagement.AcademicService.Interfaces;

namespace EnterpriseCollegeManagement.AcademicService.Services
{
    public class ExamAutoSubmitBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExamAutoSubmitBackgroundService> _logger;

        public ExamAutoSubmitBackgroundService(IServiceScopeFactory scopeFactory,ILogger<ExamAutoSubmitBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
             CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Exam auto-submit background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var examAttendanceService = scope.ServiceProvider.GetRequiredService<IExamAttendanceService>();

                    await examAttendanceService.ProcessExpiredExamsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing expired exams.");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(10),
                    stoppingToken);
            }

            _logger.LogInformation("Exam auto-submit background service stopped.");
        }
    }
}
