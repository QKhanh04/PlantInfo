using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Common.Results;
using PlantManagement.Data;
using PlantManagement.Repositories.Interfaces;

namespace PlantManagement.Repositories.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly PlantDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(PlantDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }


        public async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
        // add remove range
        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public async Task<int> Count()
        {
            return await _dbSet.CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<PagedResult<TResult>> GetPagedAsync<TResult>(
    Expression<Func<T, bool>>? filter = null,
    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
    Func<IQueryable<T>, IQueryable<T>>? include = null,
    Expression<Func<T, bool>>? keywordFilter = null,
    List<(List<int> Ids, Expression<Func<T, int>> Selector)>? idFilters = null,
    int page = 1,
    int pageSize = 10,
    Func<T, TResult>? selector = null)
        {
            IQueryable<T> query = _dbSet.AsQueryable();

            if (include != null)
                query = include(query);

            if (filter != null)
                query = query.Where(filter);

            if (keywordFilter != null)
                query = query.Where(keywordFilter);

            // ✅ Áp dụng các bộ lọc ID
            if (idFilters != null)
            {
                foreach (var (ids, selectorExpr) in idFilters)
                {
                    if (ids != null && ids.Any())
                    {
                        // Dùng Expression để EF có thể dịch sang SQL
                        query = query.Where(BuildContainsExpression(selectorExpr, ids));
                    }
                }
            }

            var total = await query.CountAsync();

            if (orderBy != null)
                query = orderBy(query);

            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            var data = await query.ToListAsync();

            List<TResult> resultList = selector != null
                ? data.Select(selector).ToList()
                : data.Cast<TResult>().ToList();

            return new PagedResult<TResult>
            {
                Items = resultList,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = total,
            };
        }

        private static Expression<Func<T, bool>> BuildContainsExpression(
            Expression<Func<T, int>> selector,
            List<int> ids)
        {
            var parameter = selector.Parameters.First();
            var body = Expression.Call(
                Expression.Constant(ids),
                typeof(List<int>).GetMethod("Contains", new[] { typeof(int) })!,
                selector.Body
            );
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }





    }

}