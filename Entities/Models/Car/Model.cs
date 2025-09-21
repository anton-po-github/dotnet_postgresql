using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_postgresql.Entities.Models.Car
{
    public class Model : BaseEntity
    {
        public string ModelName { get; set; } = "";
        public int MakeId { get; set; }

        [ForeignKey("MakeId")]
        public virtual required Make Make { get; set; }

        public virtual ICollection<Car>? Cars { get; set; }
    }
}
