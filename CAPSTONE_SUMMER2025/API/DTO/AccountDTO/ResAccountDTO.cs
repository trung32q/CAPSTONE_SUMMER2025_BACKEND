﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.DTO.AccountDTO
{
    public class ResAccountDTO
    {
        public string Email { get; set; } 
        public string Password { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; } 
        public string Role { get; set; }
        public string Status { get; set; }
    }
}
