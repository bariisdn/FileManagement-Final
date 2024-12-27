using FileManagement.Data;
using FileManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace FileManagement.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly IRepository<Category> _categoryRepository;

    public CategoryController(IRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    // GET: api/category
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryRepository.GetAllAsync();
        if (!categories.Any()) return NotFound(new { Message = "No categories found." });

        return Ok(categories);
    }

    // GET: api/category/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) return NotFound(new { Message = "Category not found." });

        return Ok(category);
    }

    // POST: api/category
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] Category category)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _categoryRepository.AddAsync(category);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    // PUT: api/category/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category updatedCategory)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) return NotFound(new { Message = "Category not found." });

        category.Name = updatedCategory.Name;
        category.Description = updatedCategory.Description;

        await _categoryRepository.UpdateAsync(category);
        return Ok(new { Message = "Category updated successfully.", Category = category });
    }

    // DELETE: api/category/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) return NotFound(new { Message = "Category not found." });

        await _categoryRepository.DeleteAsync(id);
        return Ok(new { Message = "Category deleted successfully." });
    }
}