using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class ChatMessage
    {
        public int ChatMessageId { get; set; }
        public int? ChatRoomId { get; set; }
        public int? AccountId { get; set; }
        public string? MessageContent { get; set; }
        public DateTime? SentAt { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? TypeMessage { get; set; }

        public virtual Account? Account { get; set; }
        public virtual ChatRoom? ChatRoom { get; set; }
    }
}
