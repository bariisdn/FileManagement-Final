namespace FileManagement.Models;

public class Category
{
    
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }
    
    public ICollection<File> Files { get; set; } = new List<File>();
}