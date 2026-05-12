using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendFlow.DAL.Entities
{
    public class EscrowContract
    {
        public int Id { get; set; }
        public string ItemDescription { get; set; } = string.Empty;
        public decimal Amount { get; set; }

        // State Machine Status
        public EscrowState Status { get; set; } = EscrowState.Pending;

        // The user who is paying the money
        public int BuyerId { get; set; }
        [ForeignKey("BuyerId")]
        public User? Buyer { get; set; }

        // The user who is providing the good/service and getting paid
        public int SellerId { get; set; }
        [ForeignKey("SellerId")]
        public User? Seller { get; set; }
    }
}