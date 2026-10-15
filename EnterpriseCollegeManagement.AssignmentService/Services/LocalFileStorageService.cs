using EnterpriseCollegeManagement.AssignmentService.Interfaces;

namespace EnterpriseCollegeManagement.AssignmentService.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LocalFileStorageService> _logger;

        private const long MaxFileSize = 5 * 1024 * 1024;
        private const string UploadFolder = "Uploads/Assignments";

        public LocalFileStorageService(IWebHostEnvironment environment, ILogger<LocalFileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }
        public async Task<string> SavePdfAsync(IFormFile file)
        {
            if(file == null || file.Length == 0)
            {
                throw new ArgumentException("Please select a PDF file.");

            }
            if (file.Length > MaxFileSize)
                throw new ArgumentException("PDF file size cannot exceed 5 MB.");

            if (!string.Equals(Path.GetExtension(file.FileName), ".pdf",StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only PDF files are allowed.");
            }

            // pdf signature check
            var signature = new byte[5];

            await using (var inputStream = file.OpenReadStream())
            {
                var bytesRead = await inputStream.ReadAsync(signature, 0, signature.Length);

                if (bytesRead != 5 ||!signature.SequenceEqual("%PDF-"u8.ToArray())) //validate header (pdf-)
                {
                    throw new ArgumentException("The uploaded file is not a valid PDF.");
                }
            }

            var uploadPath = Path.Combine( _environment.ContentRootPath,UploadFolder);

            Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid():N}.pdf";
            var fullPath = Path.Combine(uploadPath, fileName);

            await using (var outputStream = new FileStream(  fullPath,  FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 81920,
                useAsync: true))
            {
                await file.CopyToAsync(outputStream);
            }

            _logger.LogInformation( "PDF saved successfully. FileName: {FileName}", fileName);

            return $"{UploadFolder}/{fileName}";
        }

        
        public Task DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))  
                return Task.CompletedTask;

            var normalizedPath = filePath.Replace('\\', '/');

            if (!normalizedPath.StartsWith(UploadFolder + "/",StringComparison.Ordinal))
            {
                throw new ArgumentException("Invalid upload file path.");
            }

            var rootPath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, UploadFolder));

            var fullPath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, normalizedPath));

            if (!fullPath.StartsWith(rootPath + Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid upload file path.");
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);

                _logger.LogInformation( "Stored PDF deleted. FilePath: {FilePath}",normalizedPath);
            }

            return Task.CompletedTask;
        }

    }


    
}
