using System.Diagnostics;
using System.Security.Claims;
using FileManagement.Data;
using Microsoft.AspNetCore.Mvc;
using FileManagement.Models;
using FileManagement.ViewModels;
using File = FileManagement.Models.File;

namespace FileManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRepository<File> _fileRepository;
    private readonly IRepository<Category> _categoryRepository;


    public HomeController(ILogger<HomeController> logger,IRepository<File> fileRepository, IRepository<Category> categoryRepository)
    {
        _logger = logger;
        _fileRepository = fileRepository;
        _categoryRepository = categoryRepository;

    }

    public async Task<IActionResult> Index()
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

        return View(fileViewModels);    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
}