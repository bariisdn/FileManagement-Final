using FileManagement.Models;
using Microsoft.EntityFrameworkCore;
using File = FileManagement.Models.File;

namespace FileManagement.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<File> Files { get; set; }
    public DbSet<Category> Categories { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // İlişkiler
        modelBuilder.Entity<File>()
            .HasOne(f => f.User)
            .WithMany(u => u.Files)
            .HasForeignKey(f => f.UserId);
        
        // Kategori ilişkisi
        modelBuilder.Entity<Category>()

            .HasMany(c => c.Files)

            .WithOne(f => f.Category)

            .HasForeignKey(f => f.CategoryId)

            .OnDelete(DeleteBehavior.SetNull);
    }
}