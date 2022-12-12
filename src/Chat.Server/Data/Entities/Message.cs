namespace Chat.Server.Data.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string User { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
