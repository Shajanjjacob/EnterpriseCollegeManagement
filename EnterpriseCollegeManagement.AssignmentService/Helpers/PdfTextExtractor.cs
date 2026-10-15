using UglyToad.PdfPig;

namespace EnterpriseCollegeManagement.AssignmentService.Helpers
{
    public class PdfTextExtractor
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PdfTextExtractor> _logger;

        public PdfTextExtractor(IWebHostEnvironment environment, ILogger<PdfTextExtractor> logger)
        {
            _environment = environment;
            _logger  = logger;
        }

        public Task<string> ExtractTextAsync(string fileUrl)
        {
            _logger.LogInformation("PDF text extraction started. FileUrl: {FileUrl}", fileUrl);

            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                throw new ArgumentException("PDF file path is required.");
            }

            var fileName = Path.GetFileName(fileUrl);

            if(!string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only PDF files can be processed.");
            }

            var filePath = Path.Combine(_environment.ContentRootPath, "Uploads", "Assignments", fileName);

            if (!File.Exists(filePath))  
            {
                _logger.LogWarning("PDF file not found. FileName: {FileName}", fileName);

                throw new FileNotFoundException("Submitted PDF was not found.");
            }

            using var document = PdfDocument.Open(filePath);

            var extractedText = string.Join(Environment.NewLine,document.GetPages().Select(page => page.Text));
            if (string.IsNullOrWhiteSpace(extractedText))
            {
                _logger.LogWarning("No readable text found in PDF. FileName: {FileName}", fileName);

                throw new InvalidOperationException("No readable text found. The PDF may require OCR.");
            }

            _logger.LogInformation("PDF text extraction completed. FileName: {FileName}, CharacterCount: {CharacterCount}", fileName, extractedText.Length);

            return Task.FromResult(extractedText); //return pdf text 
        }
    }
}
