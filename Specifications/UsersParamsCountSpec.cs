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

