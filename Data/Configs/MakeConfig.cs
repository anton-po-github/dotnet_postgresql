// MakeConfig.cs
using dotnet_postgresql.Entities.Models.Car;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dotnet_postgresql.Data.Configs
{
    public class MakeConfig : IEntityTypeConfiguration<Make>
    {
        public void Configure(EntityTypeBuilder<Make> builder)
        {
            // PK
            builder.HasKey(m => m.Id);

            // Table name
            builder.ToTable("Makes");

            // Columns
            builder.Property(m => m.MakeName).IsRequired().HasMaxLength(200);

            //// OwnerId from BaseEntity
            builder.Property(m => m.OwnerId).HasMaxLength(450);

            // Indexes
            builder.HasIndex(m => m.MakeName);
            builder.HasIndex(m => m.OwnerId);

            // Optional: uniqueness of the brand name (can be removed if the same name can be repeated)
            builder.HasIndex(m => m.MakeName).IsUnique();
            //builder.HasIndex(m => m.MakeName).IsUnique(false);

            // Relation: Make 1..* Models
            builder
                .HasMany(m => m.Models)
                .WithOne(mod => mod.Make)
                .HasForeignKey(mod => mod.MakeId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
