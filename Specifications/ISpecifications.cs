using System.Linq.Expressions;

public interface ISpecifications<T>
{
    Expression<Func<T, bool>> QueryableWhereExprFun { get; }
    List<Expression<Func<T, object>>> ListIncludesExprFun { get; }
    Expression<Func<T, object>> OrderByExprFun { get; }
    Expression<Func<T, object>> OrderByDescendingExprFun { get; }
    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
}

