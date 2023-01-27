using System.ComponentModel.DataAnnotations;

namespace Chat.Server.Data.Entities
{
    public class Message
    {
        public int Id { get; set; }
        [Required] public string User { get; set; } = string.Empty;
        [Required] public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
