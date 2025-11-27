using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Services;
using SmartQuiz.Models;
using System.Threading.Tasks;

namespace SmartQuiz.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;

        public MessagesController(IMessageService messageService, IUserService userService)
        {
            _messageService = messageService;
            _userService = userService;
        }

        // ------------------------------
        // PAGE PRINCIPALE DE MESSAGERIE
        // ------------------------------
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var contacts = await _messageService.GetContactsForUserAsync(userId);
            ViewBag.Contacts = contacts;

            return View();
        }

        // ------------------------------
        // API : RÉCUPÉRATION D'UNE CONVERSATION
        // ------------------------------
        [HttpGet]
        public async Task<IActionResult> Conversation(string withUserId)
        {
            var me = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(me) || string.IsNullOrEmpty(withUserId))
                return BadRequest("Invalid user id");

            // Vérification si le destinataire existe
            var otherUser = await _userService.GetByIdAsync(withUserId);
            if (otherUser == null)
                return NotFound("User not found");

            // Récupération de l'ID conversation unique
            var convId = _messageService.GetConversationId(me, withUserId);

            // Récupération des messages
            var messages = await _messageService.GetConversationAsync(convId, 500);

            // Marquer les messages reçus comme lus
            await _messageService.MarkConversationAsReadAsync(convId, me);

            return Json(messages);
        }
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var me = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (me == null) return Json(0);

            var contacts = await _messageService.GetContactsForUserAsync(me);
            int total = contacts.Sum(c => c.UnreadCount);

            return Json(total);
        }


        // ------------------------------
        // API : ENVOI D’UN MESSAGE
        // ------------------------------
        [HttpPost]
        public async Task<IActionResult> Send(string recipientId, string content)
        {
            var me = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(me))
                return Unauthorized();

            if (string.IsNullOrEmpty(recipientId))
                return BadRequest("Recipient invalid");

            if (string.IsNullOrWhiteSpace(content))
                return BadRequest("Message cannot be empty");

            // Vérification si le destinataire existe
            var recipient = await _userService.GetByIdAsync(recipientId);
            if (recipient == null)
                return NotFound("Recipient not found");

            var msg = new Message
            {
                SenderId = me,
                RecipientId = recipientId,
                Content = content.Trim()
            };

            await _messageService.SendMessageAsync(msg);

            return Ok();
        }
    }
}
