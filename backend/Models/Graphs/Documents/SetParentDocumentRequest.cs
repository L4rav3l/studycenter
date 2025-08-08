namespace StudyCenter.Models 
{
    public class SetParentDocumentRequest {
        public required int Id {get;set;}
        public int? Parent {get;set;}
    }
}