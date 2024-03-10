
using Application.Commons;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.CategoryViewModels;
using Application.ViewModels.CustomerBonsaiViewModels;
using Application.ViewModels.CustomerGardenViewModels;
using Application.ViewModels.OrderViewModels;
using Application.ViewModels.StyleViewModels;
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
            CreateMap<BonsaiModel, Bonsai>();
            CreateMap<CategoryModel, Category>();
            CreateMap<StyleModel, Style>();
            CreateMap<CustomerGardenModel, CustomerGarden>();
            CreateMap<CustomerBonsaiModel, CustomerBonsai>();
            CreateMap<OrderModel, Order>().ReverseMap();
            CreateMap<OrderViewModel, Order>().ReverseMap();
            CreateMap<UserViewModel, ApplicationUser>().ReverseMap();



        }
    }
}
