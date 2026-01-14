using MongoDB.Driver;
using SmartQuiz.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartQuiz.Services
{
    public class ChatConversationService : IChatConversationService
    {
        private readonly IMongoCollection<ChatConversation> _collection;

        public ChatConversationService(IMongoDatabase db)
        {
            _collection = db.GetCollection<ChatConversation>("ChatConversations");
        }

        public Task<List<ChatConversation>> GetUserConversationsAsync(string userId) =>
            _collection.Find(c => c.UserId == userId)
                       .SortByDescending(c => c.CreatedAt)
                       .ToListAsync();

        public Task<ChatConversation> GetByIdAsync(string id) =>
            _collection.Find(c => c.Id == id).FirstOrDefaultAsync();

        public async Task<ChatConversation> CreateAsync(ChatConversation conversation)
        {
            await _collection.InsertOneAsync(conversation);
            return conversation;
        }

        public Task AddMessageAsync(string conversationId, ChatAIMessage message) =>
        _collection.UpdateOneAsync(
            c => c.Id == conversationId,
            Builders<ChatConversation>.Update.Push(c => c.Messages, message)
        );

    }
}
