
using Application.Commons;
using Application.ViewModels.BaseTaskViewTasks;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.CategoryViewModels;
using Application.ViewModels.CustomerBonsaiViewModels;
using Application.ViewModels.CustomerGardenViewModels;
using Application.ViewModels.OrderViewModels;
using Application.ViewModels.ServiceOrderViewModels;
using Application.ViewModels.ServiceViewModels;
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
            CreateMap<BonsaiModel, Bonsai>();
            CreateMap<CategoryModel, Category>();
            CreateMap<StyleModel, Style>();
            CreateMap<CustomerGardenModel, CustomerGarden>();
            CreateMap<CustomerBonsaiModel, CustomerBonsai>().ReverseMap();
            CreateMap<BonsaiModelForCustomer, Bonsai>();
            CreateMap<ServiceOrder, OverallServiceOrderViewModel>().ReverseMap();

            CreateMap<ServiceOrder, ServiceOrderForGardenerViewModel>().ReverseMap();
            CreateMap<OrderModel, Order>().ReverseMap();
            CreateMap<OrderViewModel, Order>();
            CreateMap<Order, OrderViewModel>().ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails));
            CreateMap<UserViewModel, ApplicationUser>().ReverseMap();
            CreateMap<GardenerViewModel, ApplicationUser>().ReverseMap();
            CreateMap<BaseTaskModel, BaseTask>().ReverseMap();
            CreateMap<Service, ServiceViewModel>();
            CreateMap<ServiceModel, Service>().ReverseMap();
        }
    }
}
