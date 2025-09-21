using dotnet_postgresql.Entities.Models.Car;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dotnet_postgresql.Data.Configs
{
    public class CarConfig : IEntityTypeConfiguration<Car>
    {
        public void Configure(EntityTypeBuilder<Car> builder)
        {
            // PK
            builder.HasKey(c => c.Id);

            builder.ToTable("Cars");

            // Columns
            builder.Property(c => c.Year).IsRequired();
            builder.Property(c => c.Description).HasMaxLength(2000).HasDefaultValue("");

            // Indexes (for fast searching)
            builder.HasIndex(c => c.UserId);
            builder.HasIndex(c => c.MakeId);
            builder.HasIndex(c => c.ModelId);
            builder.HasIndex(c => c.BodyTypeId);
            builder.HasIndex(c => c.OwnerId);

            // Relationships (without an explicit navigation collection on the other side - .WithMany())
            builder
                .HasOne(c => c.User)
                .WithMany(u => u.Car)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(c => c.Make)
                .WithMany()
                .HasForeignKey(c => c.MakeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(c => c.Model)
                .WithMany()
                .HasForeignKey(c => c.ModelId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(c => c.BodyType)
                .WithMany()
                .HasForeignKey(c => c.BodyTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            // Pictures: if there is a Picture entity with a CarId field and navigation to Car
            builder
                .HasMany(c => c.Pictures)
                .WithOne(p => p.Car)
                .HasForeignKey(p => p.CarId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
