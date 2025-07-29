using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class UserCallSession
    {
        public Guid CallSessionId { get; set; }
        public int ChatRoomId { get; set; }
        public int StartedByAccountId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string Status { get; set; } = null!;
        public string? RoomToken { get; set; }

        public virtual UserChatRoom ChatRoom { get; set; } = null!;
    }
}
