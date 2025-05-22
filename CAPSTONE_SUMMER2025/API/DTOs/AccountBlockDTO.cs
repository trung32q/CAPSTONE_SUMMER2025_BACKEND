using System;

namespace API.DTOs
{
    public class BlockAccountDTO
    {
        public int BlockerAccountId { get; set; }
        public int BlockedAccountId { get; set; }
    }

    public class UnblockAccountDTO
    {
        public int BlockerAccountId { get; set; }
        public int BlockedAccountId { get; set; }
    }

    public class AccountBlockResponseDTO
    {
        public int BlockId { get; set; }
        public int BlockerAccountId { get; set; }
        public int BlockedAccountId { get; set; }
        public DateTime BlockedAt { get; set; }
    }
} 