using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class UserChatRoom
    {
        public UserChatRoom()
        {
            UserChatRoomMembers = new HashSet<UserChatRoomMember>();
            UserMessages = new HashSet<UserMessage>();
        }

        public int ChatRoomId { get; set; }
        public string Type { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<UserChatRoomMember> UserChatRoomMembers { get; set; }
        public virtual ICollection<UserMessage> UserMessages { get; set; }
    }
}
