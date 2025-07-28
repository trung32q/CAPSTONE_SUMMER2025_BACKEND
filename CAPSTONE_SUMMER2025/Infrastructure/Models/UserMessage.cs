using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class UserMessage
    {
        public int MessageId { get; set; }
        public int? ChatRoomId { get; set; }
        public int? SenderAccountId { get; set; }
        public int? SenderStartupId { get; set; }
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public string? FileType { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }

        public virtual UserChatRoom? ChatRoom { get; set; }
    }
}
