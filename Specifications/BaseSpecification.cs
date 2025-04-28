using System.Linq.Expressions;

namespace dotnet_postgresql.Specifications
{
    public class BaseSpecification<T> : ISpecifications<T>
    {
        public BaseSpecification()
        {
        }

        public BaseSpecification(Expression<Func<T, bool>> queryableWhereExprFun)
        {
            QueryableWhereExprFun = queryableWhereExprFun;
        }

        public Expression<Func<T, bool>> QueryableWhereExprFun { get; }

        public List<Expression<Func<T, object>>> ListIncludesExprFun { get; } = new List<Expression<Func<T, object>>>();

        public Expression<Func<T, object>> OrderByExprFun { get; private set; }

        public Expression<Func<T, object>> OrderByDescendingExprFun { get; private set; }

        public int Take { get; private set; }

        public int Skip { get; private set; }

        public bool IsPagingEnabled { get; private set; }

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            ListIncludesExprFun.Add(includeExpression);
        }

        protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderByExprFun = orderByExpression;
        }

        protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        {
            OrderByDescendingExprFun = orderByDescExpression;
        }

        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }
    }
}
