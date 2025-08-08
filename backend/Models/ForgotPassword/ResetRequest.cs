namespace StudyCenter.Models
{
    public class ResetRequest {
        public required string Password {get;set;}
        public required string Token {get;set;}
    }
}