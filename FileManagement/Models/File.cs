namespace FileManagement.Models;

public class File
{
    
    public int Id { get; set; }

    public required string Name { get; set; }

    public required byte[] Content { get; set; }

    public DateTime UploadedOn { get; set; }

    public string FileType { get; set; }

    public int UserId { get; set; }
    
    
    public required User User { get; set; }
    
    
    // Kategori ile ilgili ilişkiler
    public int? CategoryId { get; set; }

    public Category? Category { get; set; }
}