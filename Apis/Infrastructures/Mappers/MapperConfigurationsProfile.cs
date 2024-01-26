
using Application.ViewModels.CategoryViewModels;
using Application.ViewModels.OrderDetailModels;
using Application.ViewModels.OrderViewModels;
using Application.ViewModels.ProductViewModels;
using Application.ViewModels.SubCategoryViewModels;
using Application.ViewModels.TagViewModels;
using AutoMapper;
using Domain.Entities;

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
            CreateMap<CategoryModel, Category>();
            CreateMap<SubCategoryModel, SubCategory>();
            CreateMap<TagModel, Tag>();

            CreateMap<OrderModel, Order>().ReverseMap();
            CreateMap<OrderDetailModel, OrderDetail>().ReverseMap();


        }
    }
}
