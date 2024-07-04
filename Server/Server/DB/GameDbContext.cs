using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Migrations;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;

namespace Server.DB
{
    public class GameDbContext : DbContext
    {
        public DbSet<GameAccountDb> Accounts { get; set; }
        public DbSet<PlayerDb> Players { get; set; }
        public DbSet<ItemInfoDb> Items { get; set; }

        static readonly ILoggerFactory _logger = LoggerFactory.Create(builder => { builder.AddConsole(); });

        string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options
                //.UseLoggerFactory(_logger)
                .UseSqlServer(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<GameAccountDb>()
                .HasIndex(a => a.GameAccountDbId)
                .IsUnique();

            builder.Entity<GameAccountDb>()
                .HasIndex(a => a.AccountDbId)
                .IsUnique();

            builder.Entity<PlayerDb>()
                .HasIndex(p => p.PlayerDbId)
                .IsUnique();

            builder.Entity<PlayerDb>()
                 .HasMany(p => p.ItemSlot)
                 .WithOne(i => i.Player)
                 .HasForeignKey(i => i.PlayerDbId)
                 .IsRequired();

            builder.Entity<ItemInfoDb>()
                .HasIndex(i => i.ItemInfoDbId)
                .IsUnique();
        }
    }	
}
