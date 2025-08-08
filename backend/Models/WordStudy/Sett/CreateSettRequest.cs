namespace StudyCenter.Models
{
    public class CreateSettRequest{
        public required string Name {get;set;}
        public int? Folder {get;set;}
        public DateTime? Notification_date {get;set;}
        public DateTime? End_date {get;set;}
    }
}