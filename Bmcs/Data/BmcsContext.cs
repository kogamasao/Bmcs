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

        public BmcsContext(DbContextOptions<BmcsContext> options)
            : base(options)
        {

        }

        public DbSet<SystemAdmin> SystemAdmins { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<GameScene> GameScenes { get; set; }
        public DbSet<GameSceneDetail> GameSceneDetails { get; set; }
        public DbSet<GameSceneRunner> GameSceneRunners { get; set; }
        public DbSet<GameScoreFielder> GameScoreFielders { get; set; }
        public DbSet<GameScorePitcher> GameScorePitchers { get; set; }
        public DbSet<InningScore> InningScores { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Inquiry> Inquirys { get; set; }

        public DbSet<ResetToken> ResetTokens { get; set; }

        public DbSet<Survey> Surveys { get; set; }

        public DbSet<SurveyQuestion> SurveyQuestions { get; set; }

        public DbSet<SurveyChoice> SurveyChoices { get; set; }

        public DbSet<SurveyAnswer> SurveyAnswers { get; set; }

        public DbSet<SurveyAnswerDetail> SurveyAnswerDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.UseCollation("SQL_Latin1_General_CP1_CS_AS");

            modelBuilder.Entity<SystemAdmin>().ToTable("SystemAdmin");
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
            modelBuilder.Entity<Inquiry>().ToTable("Inquiry");
            modelBuilder.Entity<ResetToken>().ToTable("ResetToken");
            modelBuilder.Entity<Survey>().ToTable("Survey");
            modelBuilder.Entity<SurveyQuestion>().ToTable("SurveyQuestion");
            modelBuilder.Entity<SurveyChoice>().ToTable("SurveyChoice");
            modelBuilder.Entity<SurveyAnswer>().ToTable("SurveyAnswer");
            modelBuilder.Entity<SurveyAnswerDetail>().ToTable("SurveyAnswerDetail");

            modelBuilder.Entity<ResetToken>()
                .HasIndex(r => new { r.ResetTokenClass, r.TargetID });

            //回答済み判定に使用する
            modelBuilder.Entity<SurveyAnswer>()
                .HasIndex(r => new { r.SurveyID, r.UserAccountID })
                .IsUnique();

            modelBuilder.Entity<Member>()
                .HasMany(m => m.PitcherGameScenes)
                .WithOne(t => t.PitcherMember)
                .HasForeignKey(m => m.PitcherMemberID);

            modelBuilder.Entity<Member>()
                .HasMany(m => m.BatterGameScenes)
                .WithOne(t => t.BatterMember)
                .HasForeignKey(m => m.BatterMemberID);

            modelBuilder.Entity<Team>()
                .HasMany(m => m.Messages)
                .WithOne(t => t.Team)
                .HasForeignKey(m => m.TeamID);

            modelBuilder.Entity<Team>()
                .HasMany(m => m.ReplyMessages)
                .WithOne(t => t.PrivateTeam)
                .HasForeignKey(m => m.PrivateTeamID);
        }
    }
}
