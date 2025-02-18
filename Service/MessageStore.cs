using MyChatApp.Model;
using System.Collections.Concurrent;
using System.Linq;

namespace MyChatApp.Service
{
    public class ChatHistoryStore
    {
        private readonly ConcurrentBag<ChatMessage> _chatMessages = new ConcurrentBag<ChatMessage>();

        public void AddMessage(ChatMessage message)
        {
            _chatMessages.Add(message);
        }

        public IEnumerable<ChatMessage> GetMessages(string user1Id, string user2Id)
        {
            return _chatMessages
                .Where(m => (m.FromUserId == user1Id && m.ToUserId == user2Id) ||
                            (m.FromUserId == user2Id && m.ToUserId == user1Id))
                .OrderBy(m => m.Timestamp);
        }

        public IEnumerable<ChatConversation> GetConversations(string userId)
        {
            var conversations = _chatMessages
                .Where(m => m.FromUserId == userId || m.ToUserId == userId)
                .GroupBy(m => m.FromUserId == userId ? m.ToUserId : m.FromUserId)
                .Select(g => new ChatConversation
                {
                    UserId = g.Key,
                    LastMessage = g.OrderBy(m => m.Timestamp).Last().MessageContent,
                    Timestamp = g.OrderBy(m => m.Timestamp).Last().Timestamp
                });

            return conversations;
        }
    }
}
