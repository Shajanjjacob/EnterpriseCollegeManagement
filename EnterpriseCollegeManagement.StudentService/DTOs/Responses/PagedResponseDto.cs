namespace EnterpriseCollegeManagement.StudentService.DTOs.Responses
{
    public class PagedResponseDto<T>
    {
        public List<T> Items { get; set; } = new();

        public int pageNumber { get; set; }
        public int pageSize { get; set; }

        public int TotalCount { get; set; }
        public int TotalPage {  get; set; }

    }
}
