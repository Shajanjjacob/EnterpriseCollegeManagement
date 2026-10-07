namespace EnterpriseCollegeManagement.TeacherService.DTOs.Requests
{
    public class UploadProfilePhotoRequestDto
    {
        public IFormFile Photo { get; set; } = null!;
    }
}
