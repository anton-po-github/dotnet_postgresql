using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class BaseEntity
{
    [Key]
    public int Id { get; set; } // Primary Key, auto-increment

    // public Guid Id { get; set; } = Guid.NewGuid();
    public string? OwnerId { get; set; }
}

public interface IGenericService<T>
    where T : BaseEntity
{
    Task<T> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> ListAllAsync();

    Task<T> GetEntityWithSpec(ISpecifications<T> spec);

    Task<IReadOnlyList<T>> ListAsync(ISpecifications<T> spec);
    Task<int> CountAsync(ISpecifications<T> spec);
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public class GenericService<T> : IGenericService<T>
    where T : BaseEntity
{
    public readonly UsersContext _usersContext;

    public GenericService(UsersContext usersContext)
    {
        _usersContext = usersContext;
    }

    public async Task<IReadOnlyList<T>> ListAsync(ISpecifications<T> spec)
    {
        return await ApplySpecification(spec).ToListAsync();
    }

    public void Update(T entity)
    {
        throw new NotImplementedException();
    }

    private IQueryable<T> ApplySpecification(ISpecifications<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(_usersContext.Set<T>().AsQueryable(), spec);
    }

    private class SpecificationEvaluator<TEntity>
        where TEntity : BaseEntity
    {
        public static IQueryable<TEntity> GetQuery(
            IQueryable<TEntity> inputQuery,
            ISpecifications<TEntity> spec
        )
        {
            var query = inputQuery;

            if (spec.QueryableWhereExprFun != null)
            {
                query = query.Where(spec.QueryableWhereExprFun);
            }

            if (spec.OrderByExprFun != null)
            {
                query = query.OrderBy(spec.OrderByExprFun);
            }

            if (spec.OrderByDescendingExprFun != null)
            {
                query = query.OrderByDescending(spec.OrderByDescendingExprFun);
            }

            if (spec.IsPagingEnabled)
            {
                query = query.Skip(spec.Skip).Take(spec.Take);
            }

            query = spec.ListIncludesExprFun.Aggregate(
                query,
                (current, include) => current.Include(include)
            );

            return query;
        }
    }

    public void Add(T entity)
    {
        throw new NotImplementedException();
    }

    public async Task<int> CountAsync(ISpecifications<T> spec)
    {
        return await ApplySpecification(spec).CountAsync();
    }

    public void Delete(T entity)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetEntityWithSpec(ISpecifications<T> spec)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<T>> ListAllAsync()
    {
        throw new NotImplementedException();
    }
}
