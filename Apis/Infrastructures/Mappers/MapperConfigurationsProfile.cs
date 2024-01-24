
using AutoMapper;
using Application.Commons;
using Domain.Entities;
using Application.ViewModels.ProductViewModels;
using Application.ViewModels.CategoryViewModels;
using Application.ViewModels.SubCategoryViewModels;
using Application.ViewModels.OrderViewModels;

namespace Infrastructures.Mappers
{
    public class MapperConfigurationsProfile : Profile
    {
        public MapperConfigurationsProfile()
        {
            /*CreateMap<CreateChemicalViewModel, Chemical>();
            CreateMap(typeof(Pagination<>), typeof(Pagination<>));
            CreateMap<Chemical, ChemicalViewModel>()
                .ForMember(dest => dest._Id, src => src.MapFrom(x => x.Id));*/
            CreateMap<ProductModel, Product>();
            CreateMap<CategoryViewModel, Category>();
            CreateMap<SubCategoryModel, SubCategory>();


            CreateMap<OrderModel, Order>().ReverseMap();
        }
    }
}
