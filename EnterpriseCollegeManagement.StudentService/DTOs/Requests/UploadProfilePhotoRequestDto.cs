namespace EnterpriseCollegeManagement.StudentService.DTOs.Requests
{
    public class UploadProfilePhotoRequestDto
    {
        public IFormFile Photo { get; set; } = null!;
    }
}
