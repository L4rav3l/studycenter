namespace StudyCenter.Models
{
    public class CreateTodoRequest
    {
        public required DateTime Date{get;set;}
        public required string Name{get;set;}
    }
}