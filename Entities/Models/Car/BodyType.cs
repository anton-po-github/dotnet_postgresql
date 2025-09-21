namespace dotnet_postgresql.Entities.Models.Car
{
    public class BodyType : BaseEntity
    {
        public string BodyTypeName { get; set; } = "";

        // Navigation collection to Cars — useful for queries and fluent configuration
        public virtual ICollection<Car>? Cars { get; set; } = new List<Car>();
    }
}
