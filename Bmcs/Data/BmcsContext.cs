using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Bmcs.Models;

namespace Bmcs.Data
{
    public class BmcsContext : DbContext
    {
        public BmcsContext()
           : base()
        {

        }

        public BmcsContext (DbContextOptions<BmcsContext> options)
            : base(options)
        {

        }

        public DbSet<Bmcs.Models.UserAccount> UserAccounts { get; set; }
        public DbSet<Bmcs.Models.Team> Teams { get; set; }
        public DbSet<Bmcs.Models.Member> Members { get; set; }
        public DbSet<Bmcs.Models.Game> Games { get; set; }
        public DbSet<Bmcs.Models.GameScene> GameScenes { get; set; }
        public DbSet<Bmcs.Models.GameSceneDetail> GameSceneDetails { get; set; }
        public DbSet<Bmcs.Models.GameSceneRunner> GameSceneRunners { get; set; }
        public DbSet<Bmcs.Models.GameScoreFielder> GameScoreFielders { get; set; }
        public DbSet<Bmcs.Models.GameScorePitcher> GameScorePitchers { get; set; }
        public DbSet<Bmcs.Models.InningScore> InningScores { get; set; }
        public DbSet<Bmcs.Models.Message> Messages { get; set; }
        public DbSet<Bmcs.Models.Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAccount>().ToTable("UserAccount");
            modelBuilder.Entity<Team>().ToTable("Team");
            modelBuilder.Entity<Member>().ToTable("Member");
            modelBuilder.Entity<Game>().ToTable("Game");
            modelBuilder.Entity<GameScene>().ToTable("GameScene");
            modelBuilder.Entity<GameSceneDetail>().ToTable("GameSceneDetail");
            modelBuilder.Entity<GameSceneRunner>().ToTable("GameSceneRunner");
            modelBuilder.Entity<GameScoreFielder>().ToTable("GameScoreFielder");
            modelBuilder.Entity<GameScorePitcher>().ToTable("GameScorePitcher");
            modelBuilder.Entity<InningScore>().ToTable("InningScore");
            modelBuilder.Entity<Message>().ToTable("Message");
            modelBuilder.Entity<Order>().ToTable("Order");

            modelBuilder.Entity<Team>()
                  .HasMany(m => m.Messages)
                  .WithOne(t => t.Teams)
                  .HasForeignKey(m => m.TeamID);

            modelBuilder.Entity<Team>()
                .HasMany(m => m.ReplyMessages)
                .WithOne(t => t.ReplyTeams)
                .HasForeignKey(m => m.ReplyTeamID);
        }
    }
}
