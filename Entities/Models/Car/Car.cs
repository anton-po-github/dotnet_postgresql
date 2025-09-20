using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_postgresql.Entities.Models.Car
{
    public class Car : BaseEntity
    {
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int MakeId { get; set; }

        [ForeignKey("MakeId")]
        public virtual Make Make { get; set; }

        public int ModelId { get; set; }

        [ForeignKey("ModelId")]
        public virtual Model Model { get; set; }

        public int Year { get; set; }

        public string Description { get; set; } = "";

        public int BodyTypeId { get; set; }

        [ForeignKey("BodyTypeId")]
        public virtual BodyType BodyType { get; set; }
    }
}
