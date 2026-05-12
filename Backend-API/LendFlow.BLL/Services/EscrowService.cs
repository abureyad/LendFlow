using LendFlow.BLL.Interfaces;
using LendFlow.DAL.Data;
using LendFlow.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace LendFlow.BLL.Services
{
    public class EscrowService : IEscrowService
    {
        private readonly AppDbContext _context;

        public EscrowService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EscrowContract> CreateContractAsync(int buyerId, int sellerId, decimal amount, string item)
        {
            var contract = new EscrowContract
            {
                BuyerId = buyerId,
                SellerId = sellerId,
                Amount = amount,
                ItemDescription = item,
                Status = EscrowState.Pending // Rule 1: Always starts as Pending
            };

            _context.EscrowContracts.Add(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<bool> FundContractAsync(int contractId)
        {
            var contract = await _context.EscrowContracts.Include(c => c.Buyer).FirstOrDefaultAsync(c => c.Id == contractId);

            // Validation: Must exist, must be Pending, Buyer must have enough funds
            if (contract == null || contract.Status != EscrowState.Pending || contract.Buyer!.WalletBalance < contract.Amount)
                return false;

            // Move funds out of Buyer's wallet (Locking them in escrow)
            contract.Buyer.WalletBalance -= contract.Amount;

            // Advance the State Machine
            contract.Status = EscrowState.Funded;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsDeliveredAsync(int contractId)
        {
            var contract = await _context.EscrowContracts.FindAsync(contractId);

            // Validation: Seller can only deliver IF the money is locked and funded
            if (contract == null || contract.Status != EscrowState.Funded)
                return false;

            // Advance the State Machine
            contract.Status = EscrowState.Delivered;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReleaseFundsAsync(int contractId)
        {
            var contract = await _context.EscrowContracts.Include(c => c.Seller).FirstOrDefaultAsync(c => c.Id == contractId);

            // Validation: Funds can only be released if the item was delivered
            if (contract == null || contract.Status != EscrowState.Delivered)
                return false;

            // Move funds from Escrow into the Seller's wallet
            contract.Seller!.WalletBalance += contract.Amount;

            // Advance the State Machine to completion
            contract.Status = EscrowState.Released;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RaiseDisputeAsync(int contractId)
        {
            var contract = await _context.EscrowContracts.FindAsync(contractId);

            // Validation: You can only dispute a contract if the money is actually locked in the vault!
            if (contract == null || (contract.Status != EscrowState.Funded && contract.Status != EscrowState.Delivered))
                return false;

            // Freeze the contract
            contract.Status = EscrowState.Disputed;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResolveDisputeAndRefundAsync(int contractId)
        {
            // We must include the Buyer because we are touching their wallet
            var contract = await _context.EscrowContracts.Include(c => c.Buyer).FirstOrDefaultAsync(c => c.Id == contractId);

            // Validation: We can only refund if the contract is actively in a Disputed state
            if (contract == null || contract.Status != EscrowState.Disputed)
                return false;

            // Refund the money from the Escrow Vault back to the Buyer's wallet
            contract.Buyer!.WalletBalance += contract.Amount;

            // Advance state to Refunded
            contract.Status = EscrowState.Refunded;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<EscrowContract>> GetAllContractsAsync()
        {
            return await _context.EscrowContracts
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .OrderByDescending(c => c.Id)
                .ToListAsync();
        }
    }
}