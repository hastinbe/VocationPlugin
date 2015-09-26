#region Copyright & License Information
/*
 * Copyright 2015 Beau D. Hastings
 * This file is part of VocationPlugin, a plugin for TShock, which is free
 * software. It is made available to you under the terms of the GNU 
 * General Public License as published by the Free Software Foundation.
 * For more information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using Terraria;
using Terraria.ID;
using TShockAPI;
using TerrariaApi.Server;

namespace VocationPlugin
{
    [ApiVersion(1, 21)]
    public class Plugin : TerrariaPlugin
    {
        internal static Config Config;

        // List of NPCs the character cannot gain experience from
        private static readonly HashSet<int> NoXpTargets = new HashSet<int>
        {
            NPCID.BlueSlime,
            NPCID.Skeleton,
            NPCID.Bunny,
            NPCID.CaveBat,
            NPCID.Goldfish,
            NPCID.Piranha,
            NPCID.BlueJellyfish,
            NPCID.PinkJellyfish,
            NPCID.Shark,
            NPCID.Crab,
            NPCID.Bird,
            NPCID.Mimic,
            NPCID.Clinger,
            NPCID.GoldfishWalker,
            NPCID.BloodJelly,
            NPCID.FungoFish,
            NPCID.TargetDummy
        };

        public Plugin(Main game)
            : base(game)
        {
        }

        public override string Name
        {
            get
            { 
                return GetType().Assembly.GetName().Name;
            }
        }
        public override Version Version
        {
            get
            {
                return GetType().Assembly.GetName().Version;
            }
        }
        public override string Author
        {
            get
            {
                return ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(
                    GetType().Assembly, typeof(AssemblyCompanyAttribute))).Company;
            }
        }
        public override string Description
        {
            get
            {
                return ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(
                    GetType().Assembly, typeof(AssemblyDescriptionAttribute))).Description;
            }
        }

        public override void Initialize()
        {
            Config = Config.Read();

            ServerApi.Hooks.WorldSave.Register(this, OnWorldSave);
            ServerApi.Hooks.NpcStrike.Register(this, OnDamage);
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);

            Commands.InitCommands();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.WorldSave.Deregister(this, OnWorldSave);
                ServerApi.Hooks.NpcStrike.Deregister(this, OnDamage);
                ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);
            }
            base.Dispose(disposing);
        }

        public static void OnWorldSave(WorldSaveEventArgs args)
        {
            try
            {
                if (TShock.Config.AnnounceSave)
                    TShock.Utils.Broadcast(VocationPlugin.Resources.INFO_SAVING, Color.Red);

                Config.Write();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(VocationPlugin.Resources.ERR_SAVING);
                Console.ForegroundColor = ConsoleColor.Gray;
                TShock.Log.Error(VocationPlugin.Resources.ERR_SAVING);
                TShock.Log.Error(e.ToString());
            }
        }

        public static void OnJoin(JoinEventArgs args)
        {
            Contract.Requires(args != null);
            Contract.Requires(args.Who > -1);

            var player = TShock.Players[args.Who];
            int index = Config.Warrior.FindIndex(v => v.PlayerName == player.Name);

            if (index == -1)
            {
                Config.Warrior.Add(new Warrior(player.Name));
                Config.Paladin.Add(new Paladin(player.Name));
                Config.Wizard.Add(new Wizard(player.Name));
                Config.settings.Add(new Config.Settings { currentVocation = "warrior" });
            }
        }
            
        public static void OnDamage(NpcStrikeEventArgs args)
        {
            Contract.Requires(args != null);
            Contract.Requires(args.Player != null);
            Contract.Requires(args.Player.whoAmI > -1);

            if (args.Handled)
                return;

            TSPlayer player = TShock.Players[args.Player.whoAmI];

            int index = Plugin.Config.Warrior.FindIndex(p => p.PlayerName == player.Name);
            if (index == -1)
                return;

            Vocation character = Vocation.getVocation(index, Plugin.Config.settings[index].currentVocation);

            // Can only get XP from non-prohibited NPCs
            if (!NoXpTargets.Contains(args.Npc.netID))
            {
                character.XP += Vocation.getDamageXp(args.Damage);
                if (character.XP >= Vocation.getLevelXp(character.Level + 1))
                {
                    character.Level++;
                    character.XP = 0;
                    player.SendInfoMessage(VocationPlugin.Resources.INFO_LEVELUP, character.Level);
                }
            }

            character.InflictDamage(player, args);
        }

        public void OnConfigRead(ConfigFile file)
        {
        }
    }
}
