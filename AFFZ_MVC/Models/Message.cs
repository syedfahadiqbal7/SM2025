using AFFZ_Customer.Utils;

namespace AFFZ_Customer.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public DateTime SentAt { get; set; }

        [EmailOrPhoneNotAllowed]
        public string Content { get; set; }
    }
}
