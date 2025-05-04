using System.Linq.Expressions;

// Базовый интерфейс спецификаций
public interface ISpecifications<T>
{
    Expression<Func<T, bool>> QueryableWhereExprFun { get; }
    List<Expression<Func<T, object>>> ListIncludesExprFun { get; }
    Expression<Func<T, object>>? OrderByExprFun { get; }
    Expression<Func<T, object>>? OrderByDescendingExprFun { get; }
    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
}

public class BaseSpecification<T> : ISpecifications<T>
{
    // Выражение WHERE с значением по умолчанию
    public Expression<Func<T, bool>> QueryableWhereExprFun { get; init; } = x => true;

    // Всегда ненулевая коллекция Include-выражений
    public List<Expression<Func<T, object>>> ListIncludesExprFun { get; } = new();

    // Сортировка может отсутствовать
    public Expression<Func<T, object>>? OrderByExprFun { get; private set; }
    public Expression<Func<T, object>>? OrderByDescendingExprFun { get; private set; }

    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    // Конструктор без параметров (использует фильтр по умолчанию)
    public BaseSpecification() { }

    // Конструктор с фильтром
    public BaseSpecification(Expression<Func<T, bool>> queryableWhereExprFun)
        : this()
    {
        QueryableWhereExprFun = queryableWhereExprFun;
    }

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
        => ListIncludesExprFun.Add(includeExpression);

    protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        => OrderByExprFun = orderByExpression;

    protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        => OrderByDescendingExprFun = orderByDescExpression;

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}

// Пример использования — наследник с фильтром
public class UsersParamsCountSpec : BaseSpecification<User>
{
    public UsersParamsCountSpec(UserSpecParams userSpecParams)
        : base(x => string.IsNullOrEmpty(userSpecParams.Search)
                  || x.FirstName.ToLower().Contains(userSpecParams.Search))
    {
    }
}

// Спецификация с сортировкой и пагинацией
public class UsersParamsSpec : BaseSpecification<User>
{
    public UsersParamsSpec(UserSpecParams userSpecParams)
        : base(x => string.IsNullOrEmpty(userSpecParams.Search)
                  || x.FirstName.ToLower().Contains(userSpecParams.Search))
    {
        AddOrderBy(x => x.FirstName);
        ApplyPaging(
            skip: userSpecParams.PageSize * (userSpecParams.PageIndex - 1),
            take: userSpecParams.PageSize);

        if (!string.IsNullOrEmpty(userSpecParams.Sort))
        {
            switch (userSpecParams.Sort)
            {
                case "priceAsc":
                    // AddOrderBy(p => p.Price);
                    break;
                case "priceDesc":
                    // AddOrderByDescending(p => p.Price);
                    break;
                default:
                    AddOrderBy(p => p.FirstName);
                    break;
            }
        }
    }
}
