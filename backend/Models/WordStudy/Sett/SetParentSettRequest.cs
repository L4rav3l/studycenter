namespace StudyCenter.Models
{
    public class SetParentSettRequest
    {
        public int? Folder_id {get;set;}
        public required int Sett_id {get;set;}
    }
}