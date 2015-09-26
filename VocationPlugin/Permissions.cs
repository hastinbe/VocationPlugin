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
using System.ComponentModel;

namespace VocationPlugin
{
    internal static class Permissions
    {
        public const string prefix = "vocation";

        [Description("User can change character class")]
        public const string changeclass = prefix + ".changeclass";

        [Description("User can view their stats")]
        public const string stats = prefix + ".stats";
    }
}