using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Models;
using SmartQuiz.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartQuiz.Controllers
{
    [Authorize]
    [Route("Chatbot")]
    public class ChatbotController : Controller
    {
        private readonly IChatConversationService _conversationService;
        private readonly IChatbotAIService _chatbotAI;

        public ChatbotController(
            IChatConversationService conversationService,
            IChatbotAIService chatbotAI)
        {
            _conversationService = conversationService;
            _chatbotAI = chatbotAI;
        }

        // ---------------- PAGE /Chatbot ----------------
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var conversations = await _conversationService.GetUserConversationsAsync(userId);
            return View(conversations);
        }

        // ---------------- POST /Chatbot/CreateConversation ----------------
        [HttpPost("CreateConversation")]
        public async Task<IActionResult> CreateConversation()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var conversation = new ChatConversation
            {
                UserId = userId,
                Role = User.IsInRole("Admin") ? "Admin" :
                       User.IsInRole("Teacher") ? "Teacher" : "Student",
                Title = "Nouvelle discussion"
            };

            await _conversationService.CreateAsync(conversation);
            return RedirectToAction(nameof(Index));
        }

        // ---------------- GET /Chatbot/GetConversation?id=xxx ----------------
        [HttpGet("GetConversation")]
        public async Task<IActionResult> GetConversation(string id)
        {
            var conversation = await _conversationService.GetByIdAsync(id);

            if (conversation == null ||
                conversation.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized();

            return Json(new { messages = conversation.Messages });
        }

        // ---------------- POST /Chatbot/Ask ----------------
        [HttpPost("Ask")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Ask([FromBody] ChatbotRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Message) ||
                string.IsNullOrWhiteSpace(request.ConversationId))
                return BadRequest("Requête invalide");

            try
            {
                await _conversationService.AddMessageAsync(
                    request.ConversationId,
                    new ChatAIMessage
                    {
                        Sender = "user",
                        Content = request.Message
                    });

                // ✅ BONNE MÉTHODE
                string aiReply = await _chatbotAI.AskChatbotAsync(request.Message, User);

                await _conversationService.AddMessageAsync(
                    request.ConversationId,
                    new ChatAIMessage
                    {
                        Sender = "ai",
                        Content = aiReply
                    });

                return Ok(new { reply = aiReply });
            }
            catch (System.Exception ex)
            {
                // 🔒 Jamais de crash serveur
                return StatusCode(500, ex.Message);
            }
        }
    }

    public class ChatbotRequest
    {
        public string ConversationId { get; set; }
        public string Message { get; set; }
    }
}
