using SmartQuiz.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartQuiz.Services
{
    public class ChatContactService : IChatContactService
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;

        public ChatContactService(IMessageService messageService, IUserService userService)
        {
            _messageService = messageService;
            _userService = userService;
        }

        public async Task<List<ContactDto>> GetContactsForUserAsync(string userId)
        {
            return await _messageService.GetContactsForUserAsync(userId);
        }
    }
}
