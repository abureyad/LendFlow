using System;
using System.Collections.Generic;
using System.Text;

namespace LendFlow.DAL.Entities
{
    public enum EscrowState
    {
        Pending = 0,    // Contract created, waiting for Buyer to deposit funds
        Funded = 1,     // Buyer deposited funds into the LendFlow Vault
        Delivered = 2,  // Seller delivered the goods/services
        Released = 3,   // Buyer approved, funds released to Seller's wallet
        Disputed = 4,   // Something went wrong, admin intervention needed
        Refunded = 5    // Money got refunded to the buyer
    }
}