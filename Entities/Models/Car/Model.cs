using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_postgresql.Entities.Models.Car
{
    public class Model : BaseEntity
    {
        public string ModelName { get; set; } = "";
        public string MakeId { get; set; } = "";

        [ForeignKey("MakeId")]
        public virtual Make Make { get; set; }
    }
}
