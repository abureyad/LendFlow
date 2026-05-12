using System;
using System.Collections.Generic;
using System.Text;
using LendFlow.DAL.Entities;

namespace LendFlow.BLL.Interfaces
{
    public interface IEscrowService
    {
        Task<EscrowContract> CreateContractAsync(int buyerId, int sellerId, decimal amount, string item);

        // The State Machine Workflow
        Task<bool> FundContractAsync(int contractId);
        Task<bool> MarkAsDeliveredAsync(int contractId);
        Task<bool> ReleaseFundsAsync(int contractId);
        Task<bool> RaiseDisputeAsync(int contractId);
        Task<bool> ResolveDisputeAndRefundAsync(int contractId);

        Task<List<LendFlow.DAL.Entities.EscrowContract>> GetAllContractsAsync();
    }
}