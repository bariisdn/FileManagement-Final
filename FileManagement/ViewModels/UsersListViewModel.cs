namespace FileManagement.ViewModels;

public class UsersListViewModel
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime CreatedOn { get; set; }
    public int FileCount { get; set; } // Dosya sayısı
}