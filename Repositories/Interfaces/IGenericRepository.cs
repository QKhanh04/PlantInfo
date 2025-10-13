using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PlantManagement.Common.Results;

namespace PlantManagement.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();

        Task<T?> GetByIdAsync(object id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<int> Count();

        IQueryable<T> Query();
        Task SaveChangesAsync();
        Task<PagedResult<TResult>> GetPagedAsync<TResult>(
   Expression<Func<T, bool>>? filter = null,
   Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
   Func<IQueryable<T>, IQueryable<T>>? include = null,
   Expression<Func<T, bool>>? keywordFilter = null,
   List<(List<int> Ids, Expression<Func<T, int>> Selector)>? idFilters = null,
   int page = 1,
   int pageSize = 10,
   Func<T, TResult>? selector = null);
    }
}