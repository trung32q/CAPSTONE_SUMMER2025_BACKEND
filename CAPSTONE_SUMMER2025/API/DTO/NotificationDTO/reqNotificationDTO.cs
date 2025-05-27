﻿using API.Utils.Constants;

namespace API.DTO.NotificationDTO
{
    public class reqNotificationDTO
    {    
        public int UserId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
