using dotnet_postgresql.Entities.Models.Car;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dotnet_postgresql.Data.Configs
{
    public class BodyTypeConfig : IEntityTypeConfiguration<BodyType>
    {
        public void Configure(EntityTypeBuilder<BodyType> builder)
        {
            // Primary key
            builder.HasKey(bt => bt.Id);

            // Table name
            builder.ToTable("BodyTypes");

            // Columns
            builder.Property(bt => bt.BodyTypeName).IsRequired().HasMaxLength(200);

            // OwnerId from BaseEntity (if used to store user id)
            builder.Property(bt => bt.OwnerId).HasMaxLength(450);

            // Indexes for fast lookup
            builder.HasIndex(bt => bt.BodyTypeName);
            builder.HasIndex(bt => bt.OwnerId);

            // Optional: unique constraint on name if you want strict uniqueness
            // builder.HasIndex(bt => bt.BodyTypeName).IsUnique();

            // Relation: one BodyType has many Cars
            builder
                .HasMany(bt => bt.Cars)
                .WithOne(c => c.BodyType)
                .HasForeignKey(c => c.BodyTypeId)
                .OnDelete(DeleteBehavior.NoAction); // protect cars from accidental cascade delete
        }
    }
}
