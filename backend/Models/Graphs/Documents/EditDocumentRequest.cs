namespace StudyCenter.Models 
{
    public class EditDocumentRequest {
        public required string Title {get;set;}
        public required string Text {get;set;}
        public required int Id {get;set;}
    }
}