// src/ChatApp.Application/Mapping/AuthMappingProfile.cs
using System.Linq;
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

        CreateMap<Message, MessageDTO>();

        CreateMap<Conversation, UserConversationsResponseDTO>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src =>
                src.IsGroup ? src.Title : src.Participants.Select(p => p.User.Name).FirstOrDefault()))
            .ForMember(dest => dest.LastMessage, opt => opt.MapFrom(src => src.LastMessage));
    }
}