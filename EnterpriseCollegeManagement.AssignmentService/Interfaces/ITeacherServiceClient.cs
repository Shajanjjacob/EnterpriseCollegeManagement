namespace EnterpriseCollegeManagement.AssignmentService.Interfaces
{
    public interface ITeacherServiceClient
    {
        Task<bool> IsTeacherAssignedToCourseSubjectAsync(int teacherId, int courseSubjectId);

        Task<int?> GetTeacherIdByUserIdAsync(string userId);

    }
}
