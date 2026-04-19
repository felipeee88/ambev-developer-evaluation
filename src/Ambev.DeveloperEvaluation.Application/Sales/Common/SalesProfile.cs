using Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.Commands.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.Commands.UpdateSale;
using Ambev.DeveloperEvaluation.Application.Sales.Queries.GetSaleById;
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

        // Per-slice derived Results — AutoMapper doesn't auto-derive maps for
        // subclasses, so we declare each one explicitly.
        CreateMap<Sale, CreateSaleResult>().IncludeBase<Sale, SaleResult>();
        CreateMap<Sale, UpdateSaleResult>().IncludeBase<Sale, SaleResult>();
        CreateMap<Sale, CancelSaleResult>().IncludeBase<Sale, SaleResult>();
        CreateMap<Sale, CancelSaleItemResult>().IncludeBase<Sale, SaleResult>();
        CreateMap<Sale, GetSaleByIdResult>().IncludeBase<Sale, SaleResult>();
    }
}
