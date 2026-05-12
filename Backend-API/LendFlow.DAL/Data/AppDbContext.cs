using System;
using System.Collections.Generic;
using System.Text;
using LendFlow.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace LendFlow.DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<EscrowContract> EscrowContracts { get; set; }

        // We need this Fluent API configuration because we have two Foreign Keys
        // pointing to the exact same User table. This tells SQL Server not to panic.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EscrowContract>()
                .HasOne(e => e.Buyer)
                .WithMany()
                .HasForeignKey(e => e.BuyerId)
                .OnDelete(DeleteBehavior.Restrict); // Prevents cascading delete loops

            modelBuilder.Entity<EscrowContract>()
                .HasOne(e => e.Seller)
                .WithMany()
                .HasForeignKey(e => e.SellerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}