
using Application.Commons;
using Application.ViewModels.CategoryViewModels;
using Application.ViewModels.OrderDetailModels;
using Application.ViewModels.OrderViewModels;
using Application.ViewModels.ProductTagViewModels;
using Application.ViewModels.ProductViewModels;
using Application.ViewModels.ServiceModels;
using Application.ViewModels.SubCategoryViewModels;
using Application.ViewModels.TagViewModels;
using Application.ViewModels.TasksViewModels;
using Application.ViewModels.UserViewModels;
using AutoMapper;
using Domain.Entities;

namespace Infrastructures.Mappers
{
    public class MapperConfigurationsProfile : Profile
    {
        public MapperConfigurationsProfile()
        {
            CreateMap(typeof(Pagination<>), typeof(Pagination<>));
            /*CreateMap<CreateChemicalViewModel, Chemical>();
            CreateMap(typeof(Pagination<>), typeof(Pagination<>));
            CreateMap<Chemical, ChemicalViewModel>()
                .ForMember(dest => dest._Id, src => src.MapFrom(x => x.Id));*/
            CreateMap<ProductModel, Product>();
            CreateMap<CategoryModel, Category>();
            CreateMap<SubCategoryModel, SubCategory>();
            CreateMap<TagModel, Tag>();
            CreateMap<ProductTagModel, ProductTag>();
            CreateMap<ServiceModel, Service>();
            CreateMap<OrderModel, Order>().ReverseMap();
            CreateMap<OrderDetailModel, OrderDetail>().ReverseMap();
            CreateMap<OrderViewModel, Order>().ReverseMap();
            CreateMap<TasksModel, Tasks>().ReverseMap();
            CreateMap<UserViewModel, ApplicationUser>().ReverseMap();



        }
    }
}
