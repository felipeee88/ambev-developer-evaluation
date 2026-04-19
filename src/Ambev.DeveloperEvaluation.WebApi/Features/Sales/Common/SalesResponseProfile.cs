using Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.Commands.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.Commands.UpdateSale;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Queries.GetSaleById;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;

public sealed class SalesResponseProfile : Profile
{
    public SalesResponseProfile()
    {
        CreateMap<CustomerInfoDto, CustomerDto>();
        CreateMap<BranchInfoDto, BranchDto>();
        CreateMap<ProductInfoDto, ProductDto>();
        CreateMap<SaleItemResult, SaleItemResponse>();
        CreateMap<SaleResult, SaleResponse>()
            .IncludeAllDerived();
        CreateMap<CreateSaleResult, SaleResponse>();
        CreateMap<UpdateSaleResult, SaleResponse>();
        CreateMap<CancelSaleResult, SaleResponse>();
        CreateMap<CancelSaleItemResult, SaleResponse>();
        CreateMap<GetSaleByIdResult, SaleResponse>();
        CreateMap<SaleSummary, SaleResponse>();
    }
}
