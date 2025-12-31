using AutoMapper;
using FileStorage.Domain.Entities;

namespace FileStorage.Application.DTOs.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<StoredObject, StoredObjectDto>();
        }
    }
}
