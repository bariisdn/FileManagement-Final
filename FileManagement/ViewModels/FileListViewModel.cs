namespace FileManagement.ViewModels
{
    public class FileListViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public DateTime UploadedOn { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }  
        public string CategoryName { get; set; } // Add this property for category name

    }
}