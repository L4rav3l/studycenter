namespace StudyCenter.Models
{
    public class CreateCategoryRequest {
        public required string Name {get;set;}
        public int? Parent {get;set;}
        public required int Main_parent {get;set;}
    }
}