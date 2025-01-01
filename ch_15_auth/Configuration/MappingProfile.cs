using AutoMapper;
using Entities;
using Entities.DTOs;

namespace Configuration;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Book, BookDto>().ReverseMap();
        CreateMap<BookDtoForInsertion, Book>().ReverseMap();
        CreateMap<BookDtoForUpdate, Book>().ReverseMap();
        CreateMap<UserForRegistrationDto, User>().ReverseMap();
    }
}