using Microsoft.EntityFrameworkCore;
using File = FileManagement.Models.File;

namespace FileManagement.Data;


public class Repository<T> : IRepository<T> where T : class

{

    private readonly AppDbContext _context;

    private readonly DbSet<T> _dbSet;

  

    public Repository(AppDbContext context)

    {

        _context = context;

        _dbSet = _context.Set<T>();

    }

  
    public async Task<IEnumerable<T>> GetAllAsync()

    {

        if (typeof(T) == typeof(File))
        {
            return await _context.Set<File>()
                .Include(f => f.User)  // Include the User entity
                .ToListAsync() as IEnumerable<T>;
        }
            
        // For other entities, just return normally
        return await _dbSet.ToListAsync();
    }

  

    public async Task<T> GetByIdAsync(int id)
    {
        if (typeof(T) == typeof(File))
        {
            // Special handling for File entity, including User data
            return await _context.Set<File>()
                .Include(f => f.User)  // Include the User entity for File
                .FirstOrDefaultAsync(f => f.Id == id) as T;
        }

        // Default behavior for other entities
        return await _dbSet.FindAsync(id) ?? throw new InvalidOperationException();
    }
  

    public async Task AddAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

  

    public async Task UpdateAsync(T entity)

    {

        _dbSet.Update(entity);

        await _context.SaveChangesAsync();

    }

  

    public async Task DeleteAsync(int id)

    {

        var entity = await GetByIdAsync(id);

        if (entity != null)

        {

            _dbSet.Remove(entity);

            await _context.SaveChangesAsync();

        }

    }

}