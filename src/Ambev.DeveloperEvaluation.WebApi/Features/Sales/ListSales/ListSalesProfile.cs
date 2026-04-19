using Ambev.DeveloperEvaluation.Application.Sales.Queries.ListSales;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;

public sealed class ListSalesProfile : Profile
{
    public ListSalesProfile()
    {
        CreateMap<ListSalesRequest, ListSalesQuery>()
            .ForMember(d => d.PageSize, o => o.MapFrom(s => s.Size));

        CreateMap<ListSalesResult, PaginatedResponse<SaleResponse>>()
            .ForMember(d => d.Data, o => o.MapFrom(s => s.Data))
            .ForMember(d => d.TotalCount, o => o.MapFrom(s => s.TotalItems))
            .ForMember(d => d.CurrentPage, o => o.MapFrom(s => s.CurrentPage))
            .ForMember(d => d.TotalPages, o => o.MapFrom(s => s.TotalPages))
            .ForMember(d => d.Success, o => o.Ignore())
            .ForMember(d => d.Message, o => o.Ignore())
            .ForMember(d => d.Errors, o => o.Ignore());
    }
}
