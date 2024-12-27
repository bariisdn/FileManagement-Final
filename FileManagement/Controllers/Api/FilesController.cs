using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileManagement.Data;
using FileManagement.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using File = FileManagement.Models.File;

namespace FileManagement.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class FilesController(IRepository<File> fileRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetFiles(int page = 1, int pageSize = 10)
    {
        var files = await fileRepository.GetAllAsync();
        var pagedFiles = files.Skip((page - 1) * pageSize).Take(pageSize);

        var result = pagedFiles.Select(file => new
        {
            file.Id,
            file.Name,
            file.FileType,
            file.UploadedOn,
            file.UserId,
            UserName = file.User?.Username,
            CategoryName = file.Category?.Name
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetFile(int id)
    {
        var file = await fileRepository.GetByIdAsync(id);
        if (file == null)
            return NotFound();

        // Return file with category info, avoiding circular references
        var result = new
        {
            file.Id,
            file.Name,
            file.FileType,
            file.UploadedOn,
            file.UserId,
            UserName = file.User?.Username,
            CategoryName = file.Category?.Name // Include category name if present
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> AddFile(File file)
    {
        if (file == null || string.IsNullOrEmpty(file.Name) || file.Content.Length == 0)
        {
            return BadRequest(new { message = "Geçerli bir dosya gönderin." }); // Validate the file input
        }

        await fileRepository.AddAsync(file);
        return CreatedAtAction(nameof(GetFile), new { id = file.Id }, file);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateFile(int id, File file)
    {
        if (id != file.Id)
            return BadRequest();

        await fileRepository.UpdateAsync(file);
        return NoContent();
    }

    [HttpDelete("{id}")]
public async Task<ActionResult> DeleteFile(int id)
{
    try
    {
        var file = await fileRepository.GetByIdAsync(id); // Retrieve the file to be deleted
        if (file == null)
        {
            return NotFound(new { message = "Dosya bulunamadı." });
        }

        // Add additional authorization check if needed
    

        await fileRepository.DeleteAsync(id); // Delete the file
        return Ok(new { message = "Dosya başarıyla silindi." });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Hata (DeleteFile): {ex.Message}"); // Error logging
        return StatusCode(500, new { message = "Dosya silinirken bir hata oluştu." });
    }
}
}