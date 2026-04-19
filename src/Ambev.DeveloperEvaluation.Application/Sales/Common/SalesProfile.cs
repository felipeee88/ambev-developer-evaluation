using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

/// <summary>
/// Shared domain-to-DTO maps used by every Sales slice.
/// </summary>
public sealed class SalesProfile : Profile
{
    public SalesProfile()
    {
        CreateMap<CustomerInfo, CustomerInfoDto>();
        CreateMap<BranchInfo, BranchInfoDto>();
        CreateMap<ProductInfo, ProductInfoDto>();
        CreateMap<SaleItem, SaleItemResult>();
        CreateMap<Sale, SaleResult>();
        CreateMap<Sale, SaleSummary>();
    }
}
