namespace StudyCenter.Models 
{
    public class SetParentCategoryRequest {
        public required int Id {get;set;}
        public int? Parent {get;set;}
    }
}