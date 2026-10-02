namespace EnterpriseCollegeManagement.AcademicService.DTOs.Responses
{
    public class PagedResponseDto<T>
    {
        public List<T> Items { get; set; } = new();  //t => courseresponsedto,subjectresponsedto,.....

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalRecords { get; set; }

        public int TotalPages { get; set; }
    }
}
