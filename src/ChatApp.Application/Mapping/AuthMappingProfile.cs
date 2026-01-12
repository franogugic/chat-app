using AutoMapper;
using ChatApp.Application.DTO_s;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Mapping;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<User, CreateUserResponseDTO>();
    }
}