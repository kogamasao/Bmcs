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
        public BmcsContext (DbContextOptions<BmcsContext> options)
            : base(options)
        {
        }

        public DbSet<Bmcs.Models.User> Users { get; set; }
        public DbSet<Bmcs.Models.Team> Teams { get; set; }
        public DbSet<Bmcs.Models.Member> Members { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Team>().ToTable("Team");
            modelBuilder.Entity<Member>().ToTable("Member");
        }


    }
}
