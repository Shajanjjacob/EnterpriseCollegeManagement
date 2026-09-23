namespace EnterpriseCollegeManagement.AcademicService.Entities
{
    public class CourseSubject
    {
        //many to many (course - subject)

        public int Id { get; set; }

        public int CourseId { get; set; }

        public int SubjectId { get; set; }

        public int Semester { get; set; }

        public Course Course { get; set; } = null!;

        public Subject Subject { get; set; } = null!;

        public ICollection<Exam> Exams { get; set; } = new List<Exam>(); //one coursesubject have mant xams 

    }
}
