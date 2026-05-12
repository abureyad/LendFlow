using System;
using System.Collections.Generic;
using System.Text;

namespace LendFlow.DAL.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Simulates a bank account or digital wallet
        public decimal WalletBalance { get; set; }
    }
}