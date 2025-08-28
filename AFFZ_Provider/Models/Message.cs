using AFFZ_Provider.Utils;

namespace AFFZ_Provider.Models
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
