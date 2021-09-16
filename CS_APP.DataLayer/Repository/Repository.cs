using System.Linq;
using System.Threading.Tasks;
using CS_APP.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace CS_APP.DataLayer.Repository
{
     public class Repository<T> : IRepository<T> where T : Entity
     {
          private readonly ApplicationContext _context;
          private readonly DbSet<T> _dbSet;

          public Repository(ApplicationContext context)
          {
               _context = context;
               _dbSet = context.Set<T>();
          }

          public IQueryable<T> GetAll()
          {
               return _dbSet.AsNoTracking();
          }

          public async Task<T> GetById(int id)
          {
               return await _dbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == id);
          }

          public async Task Create(T entity)
          {
               await _dbSet.AddAsync(entity);
          }

          public void Update(T entity)
          {
               _dbSet.Update(entity);
          }

          public async Task Delete(int id)
          {
               var entity = await GetById(id);
               _dbSet.Remove(entity);
          }

          public async Task SaveChanges()
          {
               await _context.SaveChangesAsync();
          }
     }
}