using MyChatApp.Model;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MyChatApp.Service;


namespace MyChatApp
{
    public class WebSocketHandler
    {
        private readonly ChatHistoryStore _chatHistoryStore;
        private readonly ConcurrentDictionary<string, List<WebSocket>> _connections = new ConcurrentDictionary<string, List<WebSocket>>();

        public WebSocketHandler(ChatHistoryStore chatHistoryStore)
        {
            _chatHistoryStore = chatHistoryStore;
        }

        public async Task HandleConnection(WebSocket socket, string userId)
        {
            _connections.AddOrUpdate(userId, new List<WebSocket> { socket }, (key, existingList) => {
                existingList.Add(socket);
                return existingList;
            });

            await ReceiveMessages(socket, userId);
        }

        public async Task ReceiveMessages(WebSocket socket, string userId)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;

            while (socket.State == WebSocketState.Open)
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var messageString = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ChatMessage message = null;

                    try
                    {
                        message = System.Text.Json.JsonSerializer.Deserialize<ChatMessage>(messageString);
                    }
                    catch (Exception)
                    {
                        await SendMessageToRecipient(socket, new ChatMessage
                        {
                            FromUserId = "system",
                            ToUserId = userId,
                            MessageContent = "Invalid message structure.",
                            Timestamp = DateTime.UtcNow
                        });
                        continue;
                    }

                    if (message.MessageContent == "typing...") 
                    {
                        if (_connections.TryGetValue(message.ToUserId, out var recipientSockets))
                        {
                            foreach (var recipientSocket in recipientSockets)
                            {
                                await SendMessageToRecipient(recipientSocket, new ChatMessage
                                {
                                    FromUserId = message.FromUserId,
                                    ToUserId = message.ToUserId,
                                    MessageContent = "User is typing...",
                                    Timestamp = DateTime.UtcNow
                                });
                            }
                        }
                    }
                    else
                    {
                        _chatHistoryStore.AddMessage(message);

                        if (_connections.TryGetValue(message.ToUserId, out var recipientSockets))
                        {
                            foreach (var recipientSocket in recipientSockets)
                            {
                                await SendMessageToRecipient(recipientSocket, message);
                            }
                        }
                    }
                }
            }
        }

        public async Task SendMessageToRecipient(WebSocket socket, ChatMessage message)
        {
            var messageString = System.Text.Json.JsonSerializer.Serialize(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageString);

            await socket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public void RemoveConnection(string userId)
        {
            _connections.TryRemove(userId, out _);
        }

        public List<WebSocket> GetConnections(string userId)
        {
            _connections.TryGetValue(userId, out var sockets);
            return sockets;
        }
    }
}
