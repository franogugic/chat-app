using AutoMapper;
using ChatApp.Application.DTO_s;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ChatApp.Application.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationService _conversationService;
    private readonly ILogger<MessageService> _logger;
    private readonly IMapper _mapper;

    public MessageService(IMessageRepository messageRepository, IConversationService conversationService, ILogger<MessageService> logger, IMapper mapper)
    {
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    
    public async Task<MessageDTO> SendMessage(Guid senderId, SendMessageRequestDTO request, CancellationToken cancellationToken = default)
        {
            _logger.LogCritical("DOslo do servicesa");
            if (request.ConversationId == Guid.Empty)
            {
                var existingConversation = await _conversationService.GetPrivateConversationAsync(senderId, request.RecipientId, cancellationToken);
                _logger.LogCritical("existingConversation: {ExistingConversation}", existingConversation);
                if (existingConversation != null)
                {
                    request.ConversationId = existingConversation.Id;
                }
                else
                {
                    var newConv = await _conversationService.CreatePrivateConversationAsync(senderId, request.RecipientId, cancellationToken);
                    if (newConv == null) throw new InvalidOperationException("Nije uspjelo kreiranje konverzacije.");
                    
                    request.ConversationId = newConv.Id;
                }
            }
            
            var message = await _messageRepository.SendMessage(senderId, request, cancellationToken);
            return _mapper.Map<MessageDTO>(message);
        }}