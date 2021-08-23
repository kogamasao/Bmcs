using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bmcs.Data;
using Bmcs.Models;

namespace Bmcs.Data
{
    public static class DbInitializer
    {
        public static void Initialize(BmcsContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any students.
            if (context.Teams.ToList().Any())
            {
                return;   // DB has been seeded
            }

            var userAccounts = new UserAccount[]
           {
                new UserAccount{UserAccountID="ADMIN", UserAccountName="管理者",Password="ADMIN",TeamID="YG", EmailAddress="", DeleteFLG = false,EntryDatetime=DateTime.Now, EntryUserID="ADMIN"},
           };

            context.UserAccounts.AddRange(userAccounts);

            var teams = new Team[]
            {
                new Team{TeamID="YG", TeamName="読売ジャイアンツ",RepresentativeName="原辰徳",TeamNumber=70, TeamPassword="", TeamEmailAddress="", MessageDetail="",  PublicFLG=true, DeleteFLG = false,EntryDatetime=DateTime.Now,UpdateDatetime=DateTime.Now},
                new Team{TeamID="HT",TeamName="阪神タイガース",RepresentativeName="矢野燿大",TeamNumber=69,TeamPassword="", TeamEmailAddress="", MessageDetail="",  PublicFLG=true, DeleteFLG = false,EntryDatetime=DateTime.Now,UpdateDatetime=DateTime.Now}
            };

            context.Teams.AddRange(teams);

            var members = new Member[]
            {
                new Member{TeamID="YG",MemberName="原辰徳",MemberClass=Enum.MemberClass.Manager, BatClass=null, ThrowClass=null, UniformNumber="88",MessageDetail="",DeleteFLG=false, EntryDatetime=DateTime.Now, UpdateDatetime=DateTime.Now},
                new Member{TeamID="YG",MemberName="坂本勇人",MemberClass=Enum.MemberClass.Player, BatClass=null, ThrowClass=null, UniformNumber="6",MessageDetail="",DeleteFLG=false, EntryDatetime=DateTime.Now, UpdateDatetime=DateTime.Now},
                new Member{TeamID="HT",MemberName="矢野燿大",MemberClass=Enum.MemberClass.Manager, BatClass=null, ThrowClass=null, UniformNumber="87",MessageDetail="",DeleteFLG=false, EntryDatetime=DateTime.Now, UpdateDatetime=DateTime.Now},
            };

            context.Members.AddRange(members);

            context.SaveChanges();

        }
    }
}
