using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.UnitPrice).IsRequired().HasPrecision(18, 2);
        builder.Property(i => i.Discount).IsRequired().HasPrecision(18, 2);
        builder.Property(i => i.TotalAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(i => i.Cancelled).IsRequired();

        // Shadow FK to Sale, configured by SaleConfiguration.HasMany.
        builder.Property<Guid>("SaleId").IsRequired();
        builder.HasIndex("SaleId");

        builder.OwnsOne(i => i.Product, p =>
        {
            p.Property(x => x.Id).HasColumnName("Product_Id").IsRequired();
            p.Property(x => x.Name).HasColumnName("Product_Name").IsRequired().HasMaxLength(200);
        });
    }
}
