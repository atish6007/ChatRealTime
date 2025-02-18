namespace MyChatApp.Model
{
    public class ChatMessage
    {
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public string MessageContent { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
