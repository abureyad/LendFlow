using LendFlow.DAL.Data;
using LendFlow.DAL.Entities;

namespace LendFlow.API.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // If we already have users, don't seed again
            if (context.Users.Any()) return;

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "freelance_data.csv");

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"CRITICAL: Could not find CSV at {path}");
            }

            // Read the first 50 rows (skipping the header)
            var lines = File.ReadAllLines(path).Skip(1).Take(50);
            var random = new Random();

            foreach (var line in lines)
            {
                // We split by comma. 
                // Assuming standard freelance columns like: JobTitle, ClientName, FreelancerName, Budget
                var data = line.Split(',');

                if (data.Length < 4) continue;

                var jobTitle = data[0].Trim();
                var clientName = data[1].Trim();
                var freelancerName = data[2].Trim();

                // Parse budget safely
                if (!decimal.TryParse(data[3], out decimal budget)) budget = 500.00m; // Fallback budget

                // 1. Create or Find the Buyer (Client)
                var buyer = context.Users.FirstOrDefault(u => u.FullName == clientName);
                if (buyer == null)
                {
                    buyer = new User { FullName = clientName, Email = $"{clientName.Replace(" ", "").ToLower()}@client.com", WalletBalance = 5000.00m }; // Give buyers a big wallet
                    context.Users.Add(buyer);
                    context.SaveChanges(); // Save to generate ID
                }

                // 2. Create or Find the Seller (Freelancer)
                var seller = context.Users.FirstOrDefault(u => u.FullName == freelancerName);
                if (seller == null)
                {
                    seller = new User { FullName = freelancerName, Email = $"{freelancerName.Replace(" ", "").ToLower()}@freelancer.com", WalletBalance = 0.00m }; // Sellers start at 0
                    context.Users.Add(seller);
                    context.SaveChanges(); // Save to generate ID
                }

                // 3. Create the Escrow Contract
                var contract = new EscrowContract
                {
                    ItemDescription = jobTitle,
                    Amount = budget,
                    BuyerId = buyer.Id,
                    SellerId = seller.Id,
                    Status = EscrowState.Pending // All start as pending
                };

                context.EscrowContracts.Add(contract);
            }

            context.SaveChanges();
        }
    }
}