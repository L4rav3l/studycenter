namespace StudyCenter.Models
{
    public class CreateDocumentsRequest {
        public required string Title {get;set;}
        public int? Parent {get;set;}
        public required int Main_parent {get;set;}
    }
}