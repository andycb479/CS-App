using System.Linq;
using System.Threading.Tasks;

namespace CS_APP.DataLayer.Repository
{
     public interface IRepository<T> where T: Entity
     {
          IQueryable<T> GetAll();

          Task<T> GetById(int id);

          Task Create(T entity);

          void Update(T entity);

          Task Delete(int id);

          Task SaveChanges();

     }
}
