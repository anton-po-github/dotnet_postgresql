namespace dotnet_postgresql.Entities.Models.Car
{
    public class Make : BaseEntity
    {
        public string MakeName { get; set; } = "";

        public virtual ICollection<Model>? Models { get; set; } = new List<Model>();
    }
}
