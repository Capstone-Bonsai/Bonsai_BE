using Application.ViewModels.ChemicalsViewModels;
using AutoMapper;
using Application.Commons;
using Domain.Entities;
using Application.ViewModels.ProductModels;
using Application.ViewModels.CategoryViewModels;

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
        }
    }
}
