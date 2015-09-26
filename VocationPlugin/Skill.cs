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

namespace VocationPlugin
{
    public class Attack
    {
        public int XP = 0;
        public int Level;
        public readonly AttackType Type;

        public Attack(AttackType type, int level)
        {
            Contract.Requires(level > 0);
            Contract.Requires(level <= int.MaxValue);

            Level = level;
            Type = type;
        }

        public int getAttackLevelXp(float attackMultiplier,  XpFormat fmt)
        {
            Contract.Requires(attackMultiplier > 0);
            Contract.Ensures(Contract.Result<int>() > -1);

            if (fmt == XpFormat.Percent)
            {
                var totalXp = Convert.ToInt32(Math.Round(10 * Math.Pow(attackMultiplier, Level), 0, MidpointRounding.AwayFromZero));
                var remainingXp = totalXp - XP;
                return remainingXp / totalXp * 100;
            }

            return XP;
        }
    }
}