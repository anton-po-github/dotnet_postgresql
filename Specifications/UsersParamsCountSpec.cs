using dotnet_postgresql.Entities;
using dotnet_postgresql.Helpers;

namespace dotnet_postgresql.Specifications
{
    public class UsersParamsCountSpec : BaseSpecification<User>
    {
        public UsersParamsCountSpec(UserSpecParams userSpecParams)
            : base(x => string.IsNullOrEmpty(userSpecParams.Search) || x.FirstName.ToLower().Contains(userSpecParams.Search)
            // && (!userSpecParams.BrandId.HasValue || x.ProductBrandId == userSpecParams.BrandId) &&
            //  (!userSpecParams.TypeId.HasValue || x.ProductTypeId == userSpecParams.TypeId)
            )
        {
        }
    }
}
