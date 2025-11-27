using MongoDB.Bson;
using MongoDB.Driver;
using SmartQuiz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartQuiz.Models;

namespace SmartQuiz.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMongoCollection<Message> _messages;
        private readonly IUserService _userService;

        public MessageService(IConfiguration config, IUserService userService)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var db = client.GetDatabase(config["MongoDB:DatabaseName"]);
            _messages = db.GetCollection<Message>("Messages");
            _userService = userService;

            // index for conversationId + SentAt
            var indexKeys = Builders<Message>.IndexKeys.Ascending(m => m.ConversationId).Descending(m => m.SentAt);
            _messages.Indexes.CreateOne(new CreateIndexModel<Message>(indexKeys));
        }

        public string GetConversationId(string a, string b)
        {
            // canonical: smaller id first
            if (string.CompareOrdinal(a, b) <= 0) return $"{a}_{b}";
            return $"{b}_{a}";
        }

        public async Task SendMessageAsync(Message msg)
        {
            msg.ConversationId = GetConversationId(msg.SenderId, msg.RecipientId);
            msg.SentAt = DateTime.UtcNow;
            msg.IsRead = false;
            await _messages.InsertOneAsync(msg);
        }

        public async Task<List<Message>> GetConversationAsync(string conversationId, int limit = 100)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.ConversationId, conversationId);
            var sort = Builders<Message>.Sort.Descending(m => m.SentAt);
            var list = await _messages.Find(filter).Sort(sort).Limit(limit).ToListAsync();
            list.Reverse(); // oldest -> newest
            return list;
        }

        public async Task<List<Message>> GetRecentConversationsForUserAsync(string userId, int limit = 50)
        {
            // find messages where user is sender or recipient, group by conversation
            var filter = Builders<Message>.Filter.Or(
                Builders<Message>.Filter.Eq(m => m.SenderId, userId),
                Builders<Message>.Filter.Eq(m => m.RecipientId, userId)
            );

            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument{
                    { "$or", new BsonArray{
                        new BsonDocument("SenderId", userId),
                        new BsonDocument("RecipientId", userId)
                    }}
                }),
                new BsonDocument("$sort", new BsonDocument("SentAt", -1)),
                new BsonDocument("$group", new BsonDocument{
                    { "_id", "$ConversationId" },
                    { "LastMessage", new BsonDocument("$first", "$Content") },
                    { "LastMessageAt", new BsonDocument("$first", "$SentAt") }
                }),
                new BsonDocument("$limit", limit)
            };

            var res = await _messages.Aggregate<BsonDocument>(pipeline).ToListAsync();
            // we leave conversion and contact building to controller/service consumer
            return new List<Message>(); // controller will call GetContactsForUserAsync instead
        }

        public async Task MarkAsReadAsync(string messageId, string userId)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.Id, messageId) &
                         Builders<Message>.Filter.Eq(m => m.RecipientId, userId);
            var update = Builders<Message>.Update.Set(m => m.IsRead, true);
            await _messages.UpdateOneAsync(filter, update);
        }

        public async Task MarkConversationAsReadAsync(string conversationId, string userId)
        {
            var filter = Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq(m => m.ConversationId, conversationId),
                Builders<Message>.Filter.Eq(m => m.RecipientId, userId),
                Builders<Message>.Filter.Eq(m => m.IsRead, false)
            );

            var update = Builders<Message>.Update.Set(m => m.IsRead, true);

            await _messages.UpdateManyAsync(filter, update);
        }


        public async Task<List<ContactDto>> GetContactsForUserAsync(string userId)
        {
            var me = await _userService.GetByIdAsync(userId);
            if (me == null) return new List<ContactDto>();

            var allUsers = await _userService.GetAllUsers();

            // candidate contacts depending on role
            IEnumerable<User> candidates = allUsers;

            if (me.Role == "Admin")
            {
                // admin can talk to anyone
                candidates = allUsers.Where(u => u.Id.ToString() != me.Id.ToString());
            }
            else if (me.Role == "Teacher")
            {
                // teacher can talk to admin, assigned students, other teachers
                var assignedStudents = allUsers.Where(u =>
                    u.Role == "Student" && (u.TeacherIds?.Contains(me.Id.ToString()) ?? false)
                );

                var otherTeachers = allUsers.Where(u => u.Role == "Teacher" && u.Id.ToString() != me.Id.ToString());
                var admins = allUsers.Where(u => u.Role == "Admin");

                candidates = admins.Concat(assignedStudents).Concat(otherTeachers);
            }
            else // Student
            {
                // student can talk to admin, assigned teachers, other students in same group
                var admins = allUsers.Where(u => u.Role == "Admin");
                var assignedTeachers = allUsers.Where(u => u.Role == "Teacher" && (me.TeacherIds?.Contains(u.Id.ToString()) ?? false));
                var sameGroupStudents = allUsers.Where(u => u.Role == "Student" && u.GroupId != null && u.GroupId == me.GroupId && u.Id.ToString() != me.Id.ToString());

                candidates = admins.Concat(assignedTeachers).Concat(sameGroupStudents);
            }

            // build ContactDto with last message and unread count
            var contacts = new List<ContactDto>();
            foreach (var u in candidates.DistinctBy(x => x.Id.ToString()))
            {
                var convId = GetConversationId(me.Id.ToString(), u.Id.ToString());
                var lastMsg = await _messages.Find(m => m.ConversationId == convId).SortByDescending(m => m.SentAt).FirstOrDefaultAsync();
                var unread = (int)await _messages.CountDocumentsAsync(m => m.ConversationId == convId && m.RecipientId == me.Id.ToString() && m.IsRead == false);

                contacts.Add(new ContactDto
                {
                    UserId = u.Id.ToString(),
                    DisplayName = string.IsNullOrEmpty(u.Username) ? u.Email : u.Username,
                    Role = u.Role,
                    LastMessage = lastMsg?.Content,
                    LastMessageAt = lastMsg?.SentAt,
                    UnreadCount = unread,
                    ProfileImage = u.ProfileImage
                });

            }

            // sort by last message date desc
            return contacts.OrderByDescending(c => c.LastMessageAt ?? DateTime.MinValue).ToList();
        }
    }
}
