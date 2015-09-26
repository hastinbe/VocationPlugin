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
using TShockAPI;

namespace VocationPlugin
{
    internal static class Commands
    {
        public static void InitCommands()
        {
            Action<Command> add = (cmd) => {
                TShockAPI.Commands.ChatCommands.Add(cmd);
            };

            #region Chat Commands
            add(new Command(Permissions.changeclass, Commands.ChangeVocation, "changevoc") {
                AllowServer = false,
                HelpText = VocationPlugin.Resources.HELP_CMD_CHANGEVOCATION 
            });
            add(new Command(Permissions.stats, Commands.Stats, "stats") { 
                AllowServer = false, 
                HelpText = VocationPlugin.Resources.HELP_CMD_STATS 
            });
            #endregion
        }

        public static void Stats(CommandArgs args)
        {
            Contract.Requires(args != null);
            Contract.Requires(args.Player != null);

            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage(VocationPlugin.Resources.ERR_NOT_LOGGED_IN);
                return;
            }

            int index = Plugin.Config.Warrior.FindIndex(v => v.PlayerName == args.Player.Name);
            if (index == -1)
            {
                args.Player.SendErrorMessage(VocationPlugin.Resources.ERR_NO_VOCATION);
                return;
            }

            Vocation character = Vocation.getVocation(index, Plugin.Config.settings[index].currentVocation);

            try {
                int pageNumber;
                if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
                    return;
                    
                var lines = new List<string> {
                    string.Format(VocationPlugin.Resources.INFO_STAT_LEVEL, character.Level, character.RemainingXp, character.RemainingXpPercent),
                    string.Format(VocationPlugin.Resources.INFO_STAT_ATTACK, "Magic", character.magic.Level, character.magic.getAttackLevelXp(character.MagicAttackMultiplier, XpFormat.Percent)),
                    string.Format(VocationPlugin.Resources.INFO_STAT_ATTACK, "Melee", character.melee.Level, character.melee.getAttackLevelXp(character.MeleeAttackMultiplier, XpFormat.Percent)),
                    string.Format(VocationPlugin.Resources.INFO_STAT_ATTACK, "Ranged", character.ranged.Level, character.ranged.getAttackLevelXp(character.RangedAttackMultiplier, XpFormat.Percent))
                };

                PaginationTools.SendPage(args.Player, pageNumber, lines, new PaginationTools.Settings {
                    HeaderFormat = args.Player.Name + " stats:"
                });
            }
            catch (Exception e) {
                args.Player.SendErrorMessage(e.Message);
                return;
            }
        }

        public static void ChangeVocation(CommandArgs args)
        {
            Contract.Requires(args != null);
            Contract.Requires(args.Player != null);

            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage(VocationPlugin.Resources.ERR_NOT_LOGGED_IN);
                return;
            }

            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage(VocationPlugin.Resources.USAGE_CHANGE_VOCATION, TShockAPI.Commands.Specifier);
                return;
            }

            int index = Plugin.Config.Warrior.FindIndex(v => v.PlayerName == args.Player.Name);
            if (index == -1)
            {
                args.Player.SendErrorMessage(VocationPlugin.Resources.ERR_NO_VOCATION);
                return;
            }
                
            var argVocation = args.Parameters[0].ToLower();
            try
            {
                Vocation.getVocation(index, argVocation);
            }
            catch (Exception)
            {
                args.Player.SendErrorMessage(VocationPlugin.Resources.ERR_UNKNOWN_VOCATION);
                return;
            }

            string currentVocation = Plugin.Config.settings[index].currentVocation;
            if (currentVocation == argVocation)
            {
                args.Player.SendErrorMessage(VocationPlugin.Resources.ERR_SAME_VOCATION, argVocation);
                return;
            }

            Plugin.Config.settings[index].currentVocation = argVocation;
            args.Player.SendSuccessMessage(VocationPlugin.Resources.INFO_VOCATION_CHANGED, argVocation);
        }
    }
}
