using LendFlow.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LendFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EscrowController : ControllerBase
    {
        private readonly IEscrowService _escrowService;

        public EscrowController(IEscrowService escrowService)
        {
            _escrowService = escrowService;
        }

        // GET: api/escrow/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllContracts()
        {
            var contracts = await _escrowService.GetAllContractsAsync();
            var displayData = contracts.Select(c => new {
                id = c.Id,
                item = c.ItemDescription,
                amount = c.Amount,
                status = c.Status.ToString(), // Converts 0,1,2 into "Pending", "Funded", etc.
                buyer = c.Buyer?.FullName,
                seller = c.Seller?.FullName
            });
            return Ok(displayData);
        }

        // POST: api/escrow/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateContract(int buyerId, int sellerId, decimal amount, string item)
        {
            var contract = await _escrowService.CreateContractAsync(buyerId, sellerId, amount, item);
            return Ok(new { Message = "Contract Created", Contract = contract });
        }

        // POST: api/escrow/5/fund
        [HttpPost("{id}/fund")]
        public async Task<IActionResult> FundContract(int id)
        {
            var success = await _escrowService.FundContractAsync(id);
            if (!success)
                return BadRequest("Failed to fund contract. Ensure it is Pending and the Buyer has sufficient funds.");

            return Ok(new { Message = "Funds locked securely in Escrow Vault. Status is now Funded." });
        }

        // POST: api/escrow/5/deliver
        [HttpPost("{id}/deliver")]
        public async Task<IActionResult> MarkAsDelivered(int id)
        {
            var success = await _escrowService.MarkAsDeliveredAsync(id);
            if (!success)
                return BadRequest("Failed. Escrow must be Funded before goods can be delivered.");

            return Ok(new { Message = "Goods marked as Delivered. Waiting for Buyer approval to release funds." });
        }

        // POST: api/escrow/5/release
        [HttpPost("{id}/release")]
        public async Task<IActionResult> ReleaseFunds(int id)
        {
            var success = await _escrowService.ReleaseFundsAsync(id);
            if (!success)
                return BadRequest("Failed. Cannot release funds until the contract state is Delivered.");

            return Ok(new { Message = "Funds released to Seller's digital wallet! Transaction Complete." });
        }

        // POST: api/escrow/5/dispute
        [HttpPost("{id}/dispute")]
        public async Task<IActionResult> RaiseDispute(int id)
        {
            var success = await _escrowService.RaiseDisputeAsync(id);
            if (!success)
                return BadRequest("Failed. Can only dispute contracts that are Funded or Delivered.");

            return Ok(new { Message = "Contract frozen! Status is now Disputed. Admin intervention required." });
        }

        // POST: api/escrow/5/refund
        [HttpPost("{id}/refund")]
        public async Task<IActionResult> RefundBuyer(int id)
        {
            var success = await _escrowService.ResolveDisputeAndRefundAsync(id);
            if (!success)
                return BadRequest("Failed. Cannot issue a refund unless the contract is Disputed.");

            return Ok(new { Message = "Dispute resolved. Full amount refunded to the Buyer's digital wallet." });
        }
    }
}