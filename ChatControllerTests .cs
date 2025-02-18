using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.Json;
using Xunit;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using MyChatApp.Model;

namespace MyChatApp
{
    public class ChatControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ChatControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetMessages_ShouldReturnMessageHistory()
        {
            var user1Id = "user1";
            var user2Id = "user2";

            var message = new ChatMessage
            {
                FromUserId = user1Id,
                ToUserId = user2Id,
                MessageContent = "Hello, how are you?",
                Timestamp = DateTime.UtcNow
            };

            var response = await _client.GetAsync($"api/chat/messages/{user1Id}/{user2Id}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var messages = JsonSerializer.Deserialize<ChatMessage[]>(responseString);

            Assert.NotEmpty(messages);
            Assert.Contains(messages, m => m.MessageContent == "Hello, how are you?");
        }

        [Fact]
        public async Task PostSendChat_ShouldReturnSuccess()
        {
            var message = new ChatMessage
            {
                FromUserId = "user1",
                ToUserId = "user2",
                MessageContent = "Hello!",
                Timestamp = DateTime.UtcNow
            };
            var messageJson = JsonSerializer.Serialize(message);
            var content = new StringContent(messageJson, System.Text.Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("api/chat/send-chat", content);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains(responseString, "Message sent successfully");
        }
    }
}
