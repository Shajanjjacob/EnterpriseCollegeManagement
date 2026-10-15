namespace EnterpriseCollegeManagement.AssignmentService.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SavePdfAsync(IFormFile file);

        //accepts the uploaded pdf ,validate it ,storage in loacl storage and retun path
        Task DeleteFileAsync(string filePath);
    }
}
