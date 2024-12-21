using AutoMapper;
using Entities;
using Entities.DTOs;

namespace Configuration;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<BookDtoForInsertion, Book>().ReverseMap();
        CreateMap<BookDtoForUpdate, Book>().ReverseMap();
    }
}