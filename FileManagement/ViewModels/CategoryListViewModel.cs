using System.Runtime.InteropServices.JavaScript;

namespace FileManagement.ViewModels;

public class CategoryListViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public DateTime CreatedOn { get; set; }

    public string? Description { get; set; }
    public int FileCount { get; set; }
}