namespace FileManagement.Models;

public class User
{
    public int Id { get; set; }

    public required string Username { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public string Role { get; set; } = "User"; // Varsayılan rol_

    public DateTime CreatedOn { get; set; } = DateTime.Now;
    
    
    public ICollection<File> Files { get; set; }
}