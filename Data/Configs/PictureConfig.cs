using dotnet_postgresql.Entities.Models.Car;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dotnet_postgresql.Data.Configs
{
    public class PictureConfig : IEntityTypeConfiguration<Picture>
    {
        public void Configure(EntityTypeBuilder<Picture> builder)
        {
            // Primary key
            builder.HasKey(p => p.Id);

            // Table name
            builder.ToTable("Pictures");

            // Columns
            builder.Property(p => p.Path).IsRequired().HasMaxLength(2000);

            // OwnerId from BaseEntity (e.g. if you store user id)
            builder.Property(p => p.OwnerId);

            // Indexes for fast lookup
            builder.HasIndex(p => p.CarId);
            builder.HasIndex(p => p.OwnerId);

            // Relationship: many Pictures belong to one Car
            builder
                .HasOne(p => p.Car)
                .WithMany(c => c.Pictures)
                .HasForeignKey(p => p.CarId)
                .OnDelete(DeleteBehavior.Cascade); // delete pictures when the car is deleted

            // Optional: prevent duplicate path entries per car
            builder.HasIndex(p => new { p.CarId, p.Path }).IsUnique();
        }
    }
}
