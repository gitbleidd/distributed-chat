using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Client
{
    public class Message
    {
        public string address;
    }

    public class HistMessage
    {
        public int Id { get; set; }
        public string User { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
    }


}
