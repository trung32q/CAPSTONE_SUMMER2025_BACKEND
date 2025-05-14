using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class ChatRoom
    {
        public ChatRoom()
        {
            ChatMessages = new HashSet<ChatMessage>();
            ChatRoomMembers = new HashSet<ChatRoomMember>();
        }

        public int ChatRoomId { get; set; }
        public string? RoomName { get; set; }
        public int? StartupId { get; set; }

        public virtual Startup? Startup { get; set; }
        public virtual ICollection<ChatMessage> ChatMessages { get; set; }
        public virtual ICollection<ChatRoomMember> ChatRoomMembers { get; set; }
    }
}
