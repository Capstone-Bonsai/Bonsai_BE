
using Application.Commons;
using Application.ViewModels.BaseTaskViewTasks;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.CategoryViewModels;
using Application.ViewModels.ContractViewModels;
using Application.ViewModels.CustomerBonsaiViewModels;
using Application.ViewModels.CustomerGardenViewModels;
using Application.ViewModels.OrderViewModels;
using Application.ViewModels.ServiceGardenViewModels;
using Application.ViewModels.ServiceSurchargeViewModels;
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
            /*CreateMap<CreateChemicalViewModel, Chemical>();
            CreateMap(typeof(Pagination<>), typeof(Pagination<>));
            CreateMap<Chemical, ChemicalViewModel>()
                .ForMember(dest => dest._Id, src => src.MapFrom(x => x.Id));*/
            CreateMap<BonsaiModel, Bonsai>();
            CreateMap<CategoryModel, Category>();
            CreateMap<StyleModel, Style>();
            CreateMap<ServiceGardenModel, ServiceGarden>();
            CreateMap<CustomerGardenModel, CustomerGarden>();
            CreateMap<CustomerBonsaiModel, CustomerBonsai>().ReverseMap();
            CreateMap<BonsaiModelForCustomer, Bonsai>();
            
            CreateMap<ServiceSurchargeModel, ServiceSurcharge>();
            CreateMap<Contract, ContractViewModel>().ReverseMap();
            CreateMap<OrderModel, Order>().ReverseMap();
            CreateMap<OrderViewModel, Order>();
            CreateMap<Order, OrderViewModel>().ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails));
            CreateMap<UserViewModel, ApplicationUser>().ReverseMap();
            CreateMap<GardenerViewModel, ApplicationUser>().ReverseMap();
            CreateMap<BaseTaskModel, BaseTask>().ReverseMap();
            CreateMap<ServiceViewModel, Service>();
            CreateMap<Service, ServiceViewModel>().ForMember(dest => dest.ServiceBaseTasks, opt => opt.MapFrom(src => src.ServiceBaseTasks));
            CreateMap<ServiceModel, Service>().ReverseMap();
            CreateMap<Contract, OverallContractViewModel>();
        }
    }
}
