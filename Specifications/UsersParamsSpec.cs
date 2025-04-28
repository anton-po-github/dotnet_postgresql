using dotnet_postgresql.Entities;
using dotnet_postgresql.Helpers;

namespace dotnet_postgresql.Specifications
{
    public class UsersParamsSpec : BaseSpecification<User>
    {
        public UsersParamsSpec(UserSpecParams userSpecParams)
            : base(x => string.IsNullOrEmpty(userSpecParams.Search) || x.FirstName.ToLower().Contains(userSpecParams.Search)
            // &&  (!productParams.BrandId.HasValue || x.ProductBrandId == productParams.BrandId) &&
            //   (!productParams.TypeId.HasValue || x.ProductTypeId == productParams.TypeId)
            )
        {
            //  AddInclude(x => x.ProductType);
            //  AddInclude(x => x.ProductBrand);
            AddOrderBy(x => x.FirstName);

            ApplyPaging(userSpecParams.PageSize * (userSpecParams.PageIndex - 1), userSpecParams.PageSize);

            if (!string.IsNullOrEmpty(userSpecParams.Sort))
            {
                switch (userSpecParams.Sort)
                {
                    case "priceAsc":
                        //  AddOrderBy(p => p.Price);
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

        /*  public UsersWithTypesAndBrandsSpecification(int id) : base(x => x.Id == id)
         {
              AddInclude(x => x.ProductType);
              AddInclude(x => x.ProductBrand);
         } */
    }

}
