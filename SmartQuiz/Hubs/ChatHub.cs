using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SmartQuiz.Models;
using SmartQuiz.Services;

namespace SmartQuiz.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;

        public ChatHub(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public override Task OnConnectedAsync()
        {
            // optionally: add to group by user id for push notifications
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                Groups.AddToGroupAsync(Context.ConnectionId, $"uid_{userId}");
            }
            return base.OnConnectedAsync();
        }

        public async Task SendMessage(string senderId, string recipientId, string content)
        {
            var msg = new Message
            {
                SenderId = senderId,
                RecipientId = recipientId,
                Content = content
            };

            await _messageService.SendMessageAsync(msg);

            var convId = _messageService.GetConversationId(senderId, recipientId);

            // notify recipient and sender (both)
            await Clients.Group($"uid_{recipientId}").SendAsync("ReceiveMessage", new
            {
                messageId = msg.Id,
                senderId = senderId,
                recipientId = recipientId,
                content = content,
                sentAt = msg.SentAt.ToString("o"),
                conversationId = convId
            });

            await Clients.Group($"uid_{senderId}").SendAsync("ReceiveMessage", new
            {
                messageId = msg.Id,
                senderId = senderId,
                recipientId = recipientId,
                content = content,
                sentAt = msg.SentAt.ToString("o"),
                conversationId = convId
            });
        }
    }
}
