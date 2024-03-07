
using Application.Commons;
using Application.ViewModels.CategoryViewModels;
using Application.ViewModels.OrderViewModels;
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
            CreateMap<CategoryModel, Category>();
            CreateMap<OrderModel, Order>().ReverseMap();
            CreateMap<OrderViewModel, Order>().ReverseMap();
            CreateMap<UserViewModel, ApplicationUser>().ReverseMap();



        }
    }
}
