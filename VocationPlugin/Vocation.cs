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
using System.Diagnostics.Contracts;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace VocationPlugin
{
    public enum XpFormat { Integer, Percent }
    public enum SkillType { Melee, Ranged, Magic, Summon, Consumable }

    public class Vocation
    {
        private int _level;
        private long _xp;

        public readonly string PlayerName;
        public int Level
        {
            get { return _level; }
            set
            {
                Contract.Requires(value > 0);
                Contract.Requires(value <= int.MaxValue);

                _level = value;
            }
        }
        public long XP
        {
            get { return _xp; }
            set
            {
                Contract.Requires(value >= 0);
                Contract.Requires(value <= long.MaxValue);

                _xp = value;
            }
        }

        public Skill melee;
        public Skill magic;
        public Skill ranged;

        public long RemainingXp { get { return getLevelXp(Level + 1) - XP; } }
        public long RemainingXpPercent { get { return RemainingXp / getLevelXp(Level + 1) * 100; } }

        public virtual float MagicSkillMultiplier { get { return 3f; } }
        public virtual float MeleeSkillMultiplier { get { return 2f; } }
        public virtual float RangedSkillMultiplier { get { return 2f; } }

        public Vocation(string playerName)
        {
            Contract.Requires(playerName != null);
            Contract.Requires(playerName.Length > 0);

            PlayerName = playerName;
        }

        public static Int64 getLevelXp(int lvl)
        {
            Contract.Requires(lvl > 0);
            Contract.Requires(lvl <= int.MaxValue);
            Contract.Ensures(Contract.Result<Int64>() > 0);

            return Convert.ToInt64((Math.Pow(lvl, 2) + lvl) / 2 * 100 - (lvl * 100));
        }

        public static Int64 getDamageXp(long damage)
        {
            Contract.Requires(damage > 0);
            Contract.Requires(damage <= long.MaxValue);
            Contract.Ensures(Contract.Result<Int64>() > -1);

            return (damage * 150 / 100) / 2;
        }
            
        public static Vocation getVocation(int index, string cls)
        {
            Contract.Requires(index >= 0);
            Contract.Requires(index <= int.MaxValue);
            Contract.Requires(cls != null);
            Contract.Requires(cls.Length > 0);
            Contract.Ensures(Contract.Result<Vocation>() != null);

            switch (cls)
            {
                case "wizard": return Plugin.Config.Wizard[index];
                case "paladin": return Plugin.Config.Paladin[index];
                case "warrior": return Plugin.Config.Warrior[index];
                default:
                    return null;
            }
        }

        public void InflictDamage(TSPlayer player, NpcStrikeEventArgs args)
        {
            Contract.Requires(player != null);
            Contract.Requires(args.Damage <= int.MaxValue);
            Contract.Ensures(args.Damage <= int.MaxValue);

            int bonusdmg = Level / 5;

            if (player.SelectedItem.melee)
            {
                melee.XP++;
                bonusdmg += melee.Level;

                if (melee.XP >= melee.getSkillLevelXp(MeleeSkillMultiplier, XpFormat.Integer))
                {
                    melee.XP = 0;
                    melee.Level++;
                    player.SendInfoMessage(VocationPlugin.Resources.INFO_LEVELUP_SKILL, "melee", melee.Level);
                }
            }
            else if (player.SelectedItem.magic)
            {
                magic.XP++;
                bonusdmg += magic.Level;

                if (magic.XP >= magic.getSkillLevelXp(MagicSkillMultiplier, XpFormat.Integer))
                {
                    magic.XP = 0;
                    magic.Level++;
                    player.SendInfoMessage(VocationPlugin.Resources.INFO_LEVELUP_SKILL, "magic", magic.Level);
                }
            }
            else if (player.SelectedItem.ranged)
            {
                ranged.XP++;
                bonusdmg += ranged.Level;

                if (ranged.XP >= ranged.getSkillLevelXp(RangedSkillMultiplier, XpFormat.Integer))
                {
                    ranged.XP = 0;
                    ranged.Level++;
                    player.SendInfoMessage(VocationPlugin.Resources.INFO_LEVELUP_SKILL, "ranged", ranged.Level);
                }
            }

            if (bonusdmg > 0)
            {
                args.Damage += bonusdmg;
                var color = Convert.ToInt32(Color.Gold.PackedValue);
                var msgType = Convert.ToInt32(PacketTypes.CreateCombatText);
                NetMessage.SendData(msgType, -1, -1, "+" + bonusdmg, color, args.Npc.position.X, args.Npc.position.Y);
            }
        }
    }

    internal class Warrior : Vocation
    {
        sealed public override float MagicSkillMultiplier { get { return 3f; } }
        sealed public override float MeleeSkillMultiplier { get { return 1.1f; } }
        sealed public override float RangedSkillMultiplier { get { return 1.2f; } }

        public Warrior(string playerName) : base(playerName)
        {
        }
    }

    internal class Paladin : Vocation
    {
        sealed public override float MagicSkillMultiplier { get { return 1.4f; } }
        sealed public override float MeleeSkillMultiplier { get { return 1.2f; } }
        sealed public override float RangedSkillMultiplier { get { return 1.1f; } }

        public Paladin(string playerName) : base(playerName)
        {
        }
    }

    internal class Wizard : Vocation
    {
        sealed public override float MagicSkillMultiplier { get { return 1.1f; } }
        sealed public override float MeleeSkillMultiplier { get { return 2f; } }
        sealed public override float RangedSkillMultiplier { get { return 2f; } }

        public Wizard(string playerName) : base(playerName)
        {
        }
    }
}