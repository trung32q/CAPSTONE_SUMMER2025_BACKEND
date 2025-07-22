using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class UserChatRoomMember
    {
        public int ChatRoomMemberId { get; set; }
        public int? ChatRoomId { get; set; }
        public int? AccountId { get; set; }
        public int? StartupId { get; set; }

        public virtual Account? Account { get; set; }
        public virtual UserChatRoom? ChatRoom { get; set; }
        public virtual Startup? Startup { get; set; }
    }
}
