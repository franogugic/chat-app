using AutoMapper;
using ChatApp.Application.DTO_s;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Mapping;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<User, CreateUserResponseDTO>();
        
        CreateMap<ConversationParticipant, ParticipantDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.Name));
        
        CreateMap<Conversation, ConversationDto>()
            .ForMember(dest => dest.Participants, opt => opt.MapFrom(src => src.Participants));
    }
}