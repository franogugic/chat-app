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
            .ForMember(dest => dest.Title, opt => opt.MapFrom((src, dest, destMember, context) => 
            {
                if (src.IsGroup) 
                    return src.Title;

                if (context.Items.TryGetValue("CurrentUserId", out var userIdObj) && userIdObj is Guid currentUserId)
                {
                    return src.Participants
                        .FirstOrDefault(p => p.UserId != currentUserId)?.User?.Name ?? "Unknown User";
                }

                return src.Title ?? "Chat";
            }));        
    }
}