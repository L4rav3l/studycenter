namespace StudyCenter.Models
{
    public class CreateFlashCardRequest
    {
        public required int Sett_id {get;set;}
        public required string Front {get;set;}
        public required string Back {get;set;}
    }
}