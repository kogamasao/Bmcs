﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bmcs.Data;
using Bmcs.Models;
using Bmcs.Enum;

namespace Bmcs.Data
{
    public static class DbInitializer
    {
        public static void Initialize(BmcsContext context)
        {
            //context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            //初期データ投入済
            if (context.UserAccounts.ToList().Any())
            {
                return;
            }

            var systemAdmins = new SystemAdmin[]
            {
                new SystemAdmin
                {
                      MessageDetail = "管理者よりお知らせ<br />まだリリース前です<br />アカウント、チーム、メンバー登録は作成済"
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
            };

            context.SystemAdmins.AddRange(systemAdmins);

            var userAccounts = new UserAccount[]
            {
                new UserAccount
                {
                      UserAccountID = "ADMIN"
                    , UserAccountName = "管理者"
                    , Password = "1"
                    , TeamID = "JB"
                    , EmailAddress = "proud.of.y.d@gmail.com"
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new UserAccount
                {
                      UserAccountID = "YGUser"
                    , UserAccountName = "ジャイアンツユーザ"
                    , Password = "1"
                    , TeamID = "YG"
                    , EmailAddress = ""
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new UserAccount
                {
                      UserAccountID = "HTUser"
                    , UserAccountName = "タイガースユーザ"
                    , Password = "1"
                    , TeamID = "HT"
                    , EmailAddress = "proud.of.y.d@gmail.com"
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new UserAccount
                {
                      UserAccountID = "JBUser"
                    , UserAccountName = "JAPANBRIDGEユーザ"
                    , Password = "1"
                    , TeamID = "JB"
                    , EmailAddress = "proud.of.y.d@gmail.com"
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
            };

            context.UserAccounts.AddRange(userAccounts);

            var teams = new Team[]
            {
                 new Team
                {
                      TeamID = "SYSTEM"
                    , TeamName = "システムチーム"
                    , RepresentativeName = "システム"
                    , TeamCategoryClass = TeamCategoryClass.Other
                    , UseBallClass = UseBallClass.Other
                    , ActivityBase = "システム"
                    , TeamNumber = 10
                    , TeamPassword = "SYSTEM"
                    , TeamEmailAddress = ""
                    , MessageDetail= ""
                    , PublicFLG = false
                    , SystemDataFLG = true
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Team
                {
                      TeamID = "YG"
                    , TeamName = "読売ジャイアンツ"
                    , RepresentativeName = "原　辰徳"
                    , TeamCategoryClass = TeamCategoryClass.Proffessional
                    , UseBallClass = UseBallClass.Hard
                    , ActivityBase = "東京"
                    , TeamNumber = 70
                    , TeamPassword = "1"
                    , TeamEmailAddress = ""
                    , MessageDetail= "東京ドームが本拠地です。"
                    , PublicFLG = true
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Team
                {
                      TeamID = "HT"
                    , TeamName = "阪神タイガース"
                    , RepresentativeName = "矢野　燿大"
                    , TeamCategoryClass = TeamCategoryClass.Proffessional
                    , UseBallClass = UseBallClass.Hard
                    , ActivityBase = "兵庫"
                    , TeamNumber = 70
                    , TeamPassword = "1"
                    , TeamEmailAddress = ""
                    , MessageDetail= "甲子園が本拠地です。"
                    , PublicFLG = true
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Team
                {
                      TeamID = "JB"
                    , TeamName = "JAPAN BRIDGE"
                    , RepresentativeName = "鈴木　雄三"
                    , TeamCategoryClass = TeamCategoryClass.Adult
                    , UseBallClass = UseBallClass.Rubber
                    , ActivityBase = "東京都中央区"
                    , TeamNumber = 12
                    , TeamPassword = "1"
                    , TeamEmailAddress = ""
                    , MessageDetail= "30代のチームです。"
                    , PublicFLG = false
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
            };

            context.Teams.AddRange(teams);

            var members = new Member[]
            {
                new Member
                {
                      TeamID = "SYSTEM"
                    , UniformNumber = "11"
                    , MemberName = "投手(左右未設定)"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = null
                    , BatClass = null
                    , PositionGroupClass =  PositionGroupClass.Pitcher
                    , MessageDetail = ""
                    , SystemDataFLG = true
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "SYSTEM"
                    , UniformNumber = "12"
                    , MemberName = "投手(右投手)"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = null
                    , PositionGroupClass = PositionGroupClass.Pitcher
                    , MessageDetail = ""
                    , SystemDataFLG = true
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "SYSTEM"
                    , UniformNumber = "13"
                    , MemberName = "投手(左投手)"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Left
                    , BatClass = null
                    , PositionGroupClass = PositionGroupClass.Pitcher
                    , MessageDetail = ""
                    , SystemDataFLG = true
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "SYSTEM"
                    , UniformNumber = "1"
                    , MemberName = "野手(打席未設定)"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = null
                    , BatClass = null
                    , PositionGroupClass = PositionGroupClass.Catcher
                    , MessageDetail = ""
                    , SystemDataFLG = true
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "SYSTEM"
                    , UniformNumber = "2"
                    , MemberName = "野手(右打者)"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = null
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Infielder
                    , MessageDetail = ""
                    , SystemDataFLG = true
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                 new Member
                {
                      TeamID = "SYSTEM"
                    , UniformNumber = "3"
                    , MemberName = "野手(左打者)"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = null
                    , BatClass = BatClass.Left
                    , PositionGroupClass = PositionGroupClass.Outfielder
                    , MessageDetail = ""
                    , SystemDataFLG = true
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "YG"
                    , UniformNumber = "88"
                    , MemberName = "原　辰徳"
                    , MemberClass = Enum.MemberClass.Manager
                    , ThrowClass = null
                    , BatClass = null
                    , PositionGroupClass = null
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "YG"
                    , UniformNumber = "6"
                    , MemberName = "坂本　勇人"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Infielder
                    , MessageDetail = "主将"
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "YG"
                    , UniformNumber = "18"
                    , MemberName = "菅野　智之"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Pitcher
                    , MessageDetail = "投手主将"
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "YG"
                    , UniformNumber = "8"
                    , MemberName = "丸　佳浩"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Left
                    , PositionGroupClass = PositionGroupClass.Outfielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "YG"
                    , UniformNumber = "29"
                    , MemberName = "吉川　尚輝"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Left
                    , PositionGroupClass = PositionGroupClass.Infielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "YG"
                    , UniformNumber = "22"
                    , MemberName = "小林　誠司"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Catcher
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "YG"
                    , UniformNumber = "25"
                    , MemberName = "岡本　和真"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Infielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "YG"
                    , UniformNumber = "5"
                    , MemberName = "中島　宏之"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Infielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "YG"
                    , UniformNumber = "9"
                    , MemberName = "亀井　善行"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Left
                    , PositionGroupClass = PositionGroupClass.Outfielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "YG"
                    , UniformNumber = "31"
                    , MemberName = "松原　聖弥"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Left
                    , PositionGroupClass = PositionGroupClass.Outfielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "YG"
                    , UniformNumber = "24"
                    , MemberName = "大城　卓三"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Left
                    , PositionGroupClass = PositionGroupClass.Catcher
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "YG"
                    , UniformNumber = "48"
                    , MemberName = "ウィーラー"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Outfielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "YG"
                    , UniformNumber = "00"
                    , MemberName = "湯浅　大"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Infielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                 new Member
                {
                      TeamID = "YG"
                    , UniformNumber = "0"
                    , MemberName = "増田　大輝"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Infielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "YG"
                    , UniformNumber = "20"
                    , MemberName = "戸郷　翔征"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Pitcher
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "YG"
                    , UniformNumber = "49"
                    , MemberName = "ビエイラ"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Pitcher
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "HT"
                    , UniformNumber = "88"
                    , MemberName = "矢野　燿大"
                    , MemberClass = Enum.MemberClass.Manager
                    , ThrowClass = null
                    , BatClass = null
                    , PositionGroupClass = null
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "JB"
                    , UniformNumber = "6"
                    , MemberName = "古賀　正雄"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Left
                    , PositionGroupClass = PositionGroupClass.Infielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "JB"
                    , UniformNumber = "30"
                    , MemberName = "鈴木　雄三"
                    , MemberClass = Enum.MemberClass.PlayingManager
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Catcher
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "JB"
                    , UniformNumber = "7"
                    , MemberName = "治下　竜麻"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Pitcher
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "JB"
                    , UniformNumber = "2"
                    , MemberName = "永田"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Infielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "JB"
                    , UniformNumber = "1"
                    , MemberName = "中塚"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Infielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "JB"
                    , UniformNumber = "3"
                    , MemberName = "山崎"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Infielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "JB"
                    , UniformNumber = "8"
                    , MemberName = "髙橋"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Outfielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "JB"
                    , UniformNumber = "11"
                    , MemberName = "小越"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Infielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "JB"
                    , UniformNumber = "5"
                    , MemberName = "杉田"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Outfielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "JB"
                    , UniformNumber = ""
                    , MemberName = "飯島"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Outfielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "JB"
                    , UniformNumber = "4"
                    , MemberName = "榎本"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Outfielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "JB"
                    , UniformNumber = ""
                    , MemberName = "田中"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Outfielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
                new Member
                {
                      TeamID = "JB"
                    , UniformNumber = ""
                    , MemberName = "安西"
                    , MemberClass = Enum.MemberClass.Player
                    , ThrowClass = ThrowClass.Right
                    , BatClass = BatClass.Right
                    , PositionGroupClass = PositionGroupClass.Outfielder
                    , MessageDetail = ""
                    , SystemDataFLG = false
                    , DeleteFLG = false
                    , EntryDatetime = DateTime.Now
                    , EntryUserID = "ADMIN"
                    , UpdateDatetime = DateTime.Now
                    , UpdateUserID = "ADMIN"
                },
            };

            context.Members.AddRange(members);

            context.SaveChanges();

        }
    }
}
