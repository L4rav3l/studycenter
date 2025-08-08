namespace StudyCenter.Models
{
    public class EditSettRequest
    {
        public required int Id {get;set;}
        public required string Name {get;set;}
        public DateTime? Notification_date {get;set;}
        public DateTime? End_date {get;set;}
    }
}