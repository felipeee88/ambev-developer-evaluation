using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.SaleNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => s.SaleNumber).IsUnique();

        builder.Property(s => s.SaleDate).IsRequired();
        builder.Property(s => s.TotalAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.Cancelled).IsRequired();

        builder.OwnsOne(s => s.Customer, c =>
        {
            c.Property(x => x.Id).HasColumnName("Customer_Id").IsRequired();
            c.Property(x => x.Name).HasColumnName("Customer_Name").IsRequired().HasMaxLength(200);
        });

        builder.OwnsOne(s => s.Branch, b =>
        {
            b.Property(x => x.Id).HasColumnName("Branch_Id").IsRequired();
            b.Property(x => x.Name).HasColumnName("Branch_Name").IsRequired().HasMaxLength(200);
        });

        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey("SaleId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Domain events live only in-memory — never persisted.
        builder.Ignore(s => s.DomainEvents);

        // Postgres-native optimistic concurrency: xmin is a system column kept up to date
        // on every row write, so no schema/trigger work is needed.
        builder.Property<uint>("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}
