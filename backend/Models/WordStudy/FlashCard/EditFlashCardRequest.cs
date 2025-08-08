namespace StudyCenter.Models
{
    public class EditFlashCardRequest
    {
        public required int Id {get;set;}
        public required string Front {get;set;}
        public required string Back {get;set;}
    }
}