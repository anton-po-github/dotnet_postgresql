using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_postgresql.Entities.Models.Car
{
    public class Picture : BaseEntity
    {
        public int CarId { get; set; }

        [ForeignKey("CarId")]
        public virtual Car Car { get; set; }

        public string Path { get; set; } = "";
    }
}
