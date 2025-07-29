using AutoMapper;
using MI.CRM.API.Models;
using MI.CRM.API.Dtos;

namespace MI.CRM.API.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ProjectBudgetEntry, ProjectBudgetEntryDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.Name));
    }
}
