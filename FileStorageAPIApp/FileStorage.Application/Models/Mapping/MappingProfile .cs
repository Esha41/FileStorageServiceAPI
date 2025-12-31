using AutoMapper;
using FileStorage.Domain.Entities;

namespace FileStorage.Application.Models.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<StoredObject, StoredObjectDto>();
        }
    }
}
