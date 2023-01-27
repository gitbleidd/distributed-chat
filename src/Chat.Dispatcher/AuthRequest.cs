using System.ComponentModel.DataAnnotations;

namespace Chat.Dispatcher
{
    public class AuthRequest
    {
        [Required]
        public string Login { get; } = null!;
    }
}
