namespace StudyCenter.Models
{
    public class EditTodoRequest
    {
        public required int Id{get;set;}
        public required DateTime Date{get;set;}
        public required string Name{get;set;}
    }
}