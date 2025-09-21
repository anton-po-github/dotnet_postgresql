using dotnet_postgresql.Entities.Models.Car;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dotnet_postgresql.Data.Configs
{
    public class ModelConfig : IEntityTypeConfiguration<Model>
    {
        public void Configure(EntityTypeBuilder<Model> builder)
        {
            builder.HasKey(m => m.Id);
            builder.ToTable("Models");

            builder.Property(m => m.ModelName).IsRequired().HasMaxLength(200);

            builder.HasIndex(m => m.MakeId);

            builder
                .HasOne(m => m.Make)
                .WithMany(make => make.Models)
                .HasForeignKey(m => m.MakeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(m => new { m.MakeId, m.ModelName });
        }
    }
}
