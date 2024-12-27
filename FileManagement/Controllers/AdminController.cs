using System.Security.Claims;
using FileManagement.Data;
using FileManagement.Hubs;
using FileManagement.Models;
using FileManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using File = FileManagement.Models.File;

namespace FileManagement.Controllers;
// /Controllers/AdminController.cs
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<File> _fileRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IHubContext<UserHub> _hubContext;


    public AdminController(IHubContext<UserHub> hubContext, IRepository<User> userRepository, IRepository<File> fileRepository, IRepository<Category> categoryRepository)
    {
        _hubContext = hubContext;
        _userRepository = userRepository;
        _fileRepository = fileRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IActionResult> Index()
    {
        return View(Index);

    }

    public async Task<IActionResult> Users()
    {
        var usersList = await GetUsersList(); // Kullanıcı + Dosya sayısı
        return View(usersList); // Dosya sayısı tabloya eklenmiş bir şekilde döner
    }
    
    [HttpGet]
    public async Task<IActionResult> SearchUsers(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            var allUsers = await _userRepository.GetAllAsync();
            return Json(allUsers);
        }

        var filteredUsers = await _userRepository.GetAllAsync();
        var result = filteredUsers
            .Where(u =>
                u.Username.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Json(result);
    }
    // Kullanıcı Listeleme

    // Kullanıcı Ekleme
    [HttpPost]
    public async Task<IActionResult> AddUser([FromBody] RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);  // Model doğrulama hatalarını döndür

        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            Role = model.Role,
            CreatedOn = DateTime.Now,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password) // Şifreyi hash'leyerek sakla
        };

        await _userRepository.AddAsync(user);
        return Ok(new { message = "Kullanıcı başarıyla eklendi." });
    }

    // Kullanıcı Güncelleme
    [HttpGet]
    public async Task<IActionResult> EditUser(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        var model = new EditUserViewModel()
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        };

        return Json(model); // Düzenleme için modal verisini döner
    }

 
    [HttpPut]
    public async Task<IActionResult> EditUser([FromBody] EditUserViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userRepository.GetByIdAsync(model.Id);

        user.Username = model.Username;
        user.Email = model.Email;
        user.Role = model.Role;

        await _userRepository.UpdateAsync(user);
        await _hubContext.Clients.All.SendAsync("UpdateUserRole", user.Id, user.Role);
        return Ok(new { message = "Kullanıcı başarıyla güncellendi." });
    }
    [HttpDelete]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Kullanıcı bulunamadı." });
            }

            // Sadece ID gönderin
            await _userRepository.DeleteAsync(id);
            return Ok(new { message = "Kullanıcı başarıyla silindi." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata: {ex.Message}");
            return StatusCode(500, new { message = "Kullanıcı silinirken bir hata oluştu." });
        }
    }
    // Kullanıcıları dosya sayısıyla birlikte getiren metod
    private async Task<IEnumerable<UsersListViewModel>> GetUsersList()
    {
        var users = await _userRepository.GetAllAsync();
        var fileCounts = await _fileRepository.GetAllAsync();

        // Kullanıcı başına dosya sayısını hesaplar
        var usersList = users.Select(user => new UsersListViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedOn = user.CreatedOn,
            FileCount = fileCounts.Count(file => file.UserId == user.Id) // Dosya sayısını ilişkilendir
        });

        return usersList;
    }
    
  
    
  
    // Kullanıcı işlemleri DONE
    
    public async Task<IActionResult> Files()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewBag.UserId = userId;

        // Fetch categories and pass them to the view
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = categories;

        var files = await _fileRepository.GetAllAsync();

        var fileViewModels = files.Select(f => new FileListViewModel
        {
            Id = f.Id,
            FileName = f.Name,
            FileType = f.FileType,
            UploadedOn = f.UploadedOn,
            UserId = f.UserId,
            UserName = f.User != null ? f.User.Username : "Nan",
            CategoryName = f.Category != null ? f.Category.Name : "Kategori Yok" // Add category name to the view model
        }).ToList();

        return View(fileViewModels);
    }
    // Dosya Listeleme

    public async Task<ActionResult<IEnumerable<FileListViewModel>>> FileList()
    {
        var files = await _fileRepository.GetAllAsync();

        var fileViewModels = files.Select(f => new FileListViewModel
        {
            Id = f.Id,
            FileName = f.Name,
            FileType = f.FileType,
            UploadedOn = f.UploadedOn,
            UserId = f.UserId,
            UserName = f.User?.Username ?? "Nan", // Add null-coalescing operator for better safety
        }).ToList();

        return Ok(fileViewModels);
    }
   
    [HttpPost]
public async Task<IActionResult> UploadFile(IFormFile file, int? categoryId)
{
    if (file == null || file.Length == 0)
    {
        return BadRequest(new { message = "Lütfen geçerli bir dosya seçin." });
    }

    // Optional: Check for max file size (if necessary)
    const long maxFileSize = 10 * 1024 * 1024; // 10 MB
    if (file.Length > maxFileSize)
    {
        return BadRequest(new { message = "Dosya boyutu çok büyük. Maksimum 10MB." });
    }

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user's ID
    if (string.IsNullOrEmpty(userId))
    {
        return BadRequest(new { message = "Kullanıcı girişi tespit edilemedi." });
    }

    if (!int.TryParse(userId, out int parsedUserId))
    {
        return BadRequest(new { message = "Geçersiz kullanıcı ID." });
    }

    var user = await _userRepository.GetByIdAsync(parsedUserId);
    if (user == null)
    {
        return BadRequest(new { message = "Kullanıcı bulunamadı." });
    }

    // Handle category selection
    if (categoryId.HasValue)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId.Value); // Assuming you have a Category repository
        if (category == null)
        {
            return BadRequest(new { message = "Geçersiz kategori." });
        }

        // Add the category to the file entity
        var fileEntity = new File
        {
            Name = Path.GetFileName(file.FileName),
            FileType = file.ContentType,
            UploadedOn = DateTime.Now,
            UserId = parsedUserId,
            User = user,
            CategoryId = categoryId.Value, // Set the category ID
            Category = category,           // Associate the category
            Content = new byte[] { }
        };

        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            fileEntity.Content = memoryStream.ToArray(); // Save file content
        }

        await _fileRepository.AddAsync(fileEntity);
        await _hubContext.Clients.All.SendAsync("SendFileNotification", fileEntity.Name, userId);
        return Ok(new { message = "Dosya başarıyla yüklendi." });
    }

    // If categoryId is not provided, you can either assign a default category
    // or return an error message indicating that a category is required.
    return BadRequest(new { message = "Kategori seçilmesi gerekmektedir." });
}
    
    
    
    [HttpGet("File/Download/{id}")]
    [ApiExplorerSettings(IgnoreApi = true)] 
    public async Task<IActionResult> DownloadFile(int id)
    {
        var file = await _fileRepository.GetByIdAsync(id);
        if (file == null)
        {
            return NotFound(new { message = "Dosya bulunamadı." });
        }
        if (file.Content == null || file.Content.Length == 0)
        {
            return BadRequest(new { message = "Dosya içeriği boş." });
        }
        
        return File(file.Content, file.FileType ?? "application/octet-stream", file.Name);
    }
  
// FileType detection in File class
    public string GetFileType(byte[] content)
    {
        var provider = new FileExtensionContentTypeProvider();
        if (provider.TryGetContentType("file", out string contentType))
        {
            return contentType;
        }

        // If detection fails, return a default type
        return "application/octet-stream";
    }

    // Dosya Silme
    
    [HttpDelete("File/Delete/{id}")]
    public async Task<IActionResult> DeleteFile(int id, [FromBody] DeleteFileRequest request)
    {
        try
        {
            var file = await _fileRepository.GetByIdAsync(id);
            if (file == null)
            {
                return NotFound(new { message = "Dosya bulunamadı." });
            }

            var deletingUserId = request.UserId; // The user ID sent from the frontend
            var fileName = file.Name;

            await _fileRepository.DeleteAsync(id);

            // SignalR Notification
            await _hubContext.Clients.All.SendAsync("FileDeleted", fileName, deletingUserId);

            return Ok(new { message = "Dosya başarıyla silindi." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata (Dosya Silme): {ex.Message}");
            return StatusCode(500, new { message = "Dosya silinirken bir hata oluştu." });
        }
    }

// DTO for delete request
    public class DeleteFileRequest
    {
        public int UserId { get; set; }
    }
    // Erişim İzni Hatası
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    
    // KATEGORİ İLE İLGİLİ İŞLEMLER
    // GET: api/category
    public async Task<IActionResult> CategoryManagement()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var CategoryListViewModel = categories.Select(c => new CategoryListViewModel
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        }).ToList();

        return View(CategoryListViewModel);
    }
     [HttpGet("Category")]
    public async Task<IActionResult> Categories()
    {
        var categories = await _categoryRepository.GetAllAsync();

        // Map Category to a ViewModel if needed
        var CategoryListViewModel = categories.Select(c => new CategoryListViewModel()
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        }).ToList();

        return View(CategoryListViewModel); // Pass categories to the View
    }

    // POST: admin/categories/add
    [HttpPost("Category/Add")]
    public async Task<IActionResult> AddCategory([FromForm] CategoryListViewModel categoryViewModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Lütfen geçerli bir kategori bilgisi girin." });
        }

        var category = new Category
        {
            Name = categoryViewModel.Name,
            Description = categoryViewModel.Description
        };

        await _categoryRepository.AddAsync(category);

        return Ok(new { message = "Kategori başarıyla eklendi." });
    }

    // GET: admin/categories/edit/{id}
    [HttpGet("Category/Edit/{id}")]
    public async Task<IActionResult> EditCategory(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "Kategori bulunamadı." });
        }

        var CategoryListViewModel = new CategoryListViewModel()
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            
        };

        return View(CategoryListViewModel);
    }

    // POST: admin/categories/edit/{id}
    [HttpPost("Category/Edit/{id}")]
    public async Task<IActionResult> EditCategory(int id, [FromForm] CategoryListViewModel categoryViewModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Lütfen geçerli kategori bilgisi girin." });
        }

        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "Kategori bulunamadı." });
        }

        category.Name = categoryViewModel.Name;
        category.Description = categoryViewModel.Description;

        await _categoryRepository.UpdateAsync(category);

        return Ok(new { message = "Kategori başarıyla güncellendi." });
    }

    // POST: admin/categories/delete/{id}
    [HttpPost("Category/Delete/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "Kategori bulunamadı." });
        }

        await _categoryRepository.DeleteAsync(id);

        return Ok(new { message = "Kategori başarıyla silindi." });
    }
    
}