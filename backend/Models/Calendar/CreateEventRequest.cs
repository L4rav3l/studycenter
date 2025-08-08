namespace StudyCenter.Models
{
    public class CreateEventRequest
    {
        public required DateTime Date {get;set;}
        public DateTime? Notification_date {get;set;}
        public required string Title {get;set;}
        public string? Descriptions {get;set;}
    }
}