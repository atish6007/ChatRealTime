using MyChatApp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyChatApp.Model;
using MyChatApp.Service;
using System.Linq;
using System.Threading.Tasks;

namespace MyChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly ChatHistoryStore _chatHistoryStore;
        private readonly WebSocketHandler _webSocketHandler;

        public MessageController(WebSocketHandler webSocketHandler, ChatHistoryStore chatHistoryStore)
        {
            _chatHistoryStore = chatHistoryStore;
            _webSocketHandler = webSocketHandler;
        }

     

        // POST API: Send a message from one user to another
        // Logic: This API allows a user to send a chat message to another user.
        // It accepts a message object with details like the sender (FromUserId), the recipient (ToUserId), and the message content.
        // First, it checks if the message is valid (i.e., it has the necessary fields).
        // Then, it stores the message in the chat history and attempts to send the message to the recipient if they are online via WebSocket.
        // If the recipient is not online, it returns a NotFound response.



        [HttpPost("send-chat")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage message)
        {
            if (message == null || string.IsNullOrEmpty(message.FromUserId) || string.IsNullOrEmpty(message.ToUserId) || string.IsNullOrEmpty(message.MessageContent))
            {
                return BadRequest("Message must contain FromUserId, ToUserId, and MessageContent.");
            }

            // Store the message in the chat history
            _chatHistoryStore.AddMessage(message);

            // Check if the recipient is currently online by retrieving their WebSocket connections
            var recipientSockets = _webSocketHandler.GetConnections(message.ToUserId);
            if (recipientSockets != null && recipientSockets.Any())
            {
                // If the recipient is online, send the message to their WebSocket connection(s)
                foreach (var recipientSocket in recipientSockets)
                {
                    await _webSocketHandler.SendMessageToRecipient(recipientSocket, message);
                }
                return Ok(new { Message = "Message sent successfully." });
            }
            else
            {
                // If the recipient is not online, return a NotFound response
                return NotFound("Recipient is not online.");
            }
        }



   // GET API: Retrieve chat history between two users
        // Logic: This API will return all the messages exchanged between two specific users identified by their user IDs (user1Id and user2Id).
        // It checks if any messages exist between the users, and if they do, returns them in ascending order of timestamp.
        // If no messages are found, it returns a NotFound response.
        [HttpGet("{user1Id}/{user2Id}")]
        public IActionResult GetMessages(string user1Id, string user2Id)
        {
            var messages = _chatHistoryStore.GetMessages(user1Id, user2Id);
            if (messages == null || !messages.Any())
            {
                return NotFound("No chat history found.");
            }
            return Ok(messages);
        }

    }
}
