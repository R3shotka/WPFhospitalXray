using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IRepository<T, TKey> where T : class
    {
        Task<List<T>> GetAllAsync();

        Task<T?> GetByIdAsync(TKey id);

        Task AddAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(TKey id);
    }
}
