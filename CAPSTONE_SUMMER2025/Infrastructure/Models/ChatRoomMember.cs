using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class ChatRoomMember
    {
        public int ChatGroupMembersId { get; set; }
        public int? ChatRoomId { get; set; }
        public int? AccountId { get; set; }
        public string? MemberTitle { get; set; }
        public bool? CanAdministerChannel { get; set; }
        public DateTime? JoinedAt { get; set; }

        public virtual Account? Account { get; set; }
        public virtual ChatRoom? ChatRoom { get; set; }
    }
}
