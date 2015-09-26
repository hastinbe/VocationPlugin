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
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using TShockAPI;
using Newtonsoft.Json;

namespace VocationPlugin
{
    /// <summary>
    /// Config - contains the plugin configuration that is serialized into JSON and deserialized on load.
    /// </summary>
    internal class Config
    {
        /// <summary>
        /// Path to the file containing the config.
        /// </summary>
        internal static string ConfigPath
        {
            get
            {
                return Path.Combine(TShock.SavePath, "vocations.json");
            }
        }

        [Description("Storage for all warrior vocations.")]
        public List<Warrior> Warrior = new List<Warrior>();

        [Description("Storage for all paladin vocations.")]
        public List<Paladin> Paladin = new List<Paladin>();

        [Description("Storage for all wizard vocations.")]
        public List<Wizard> Wizard = new List<Wizard>();

        [Description("Player's settings.")]
        public List<Settings> settings = new List<Settings>();

        public class Settings
        {
            public string currentVocation;
        }

        public static Config Read()
        {
            Contract.Requires(ConfigPath != null);
            Contract.Requires(ConfigPath.Length > 0);

            return Read(ConfigPath);
        }

        /// <summary>
        /// Reads a configuration file from a given path, creates it if it doesn't exist.
        /// </summary>
        /// <param name="path">string path</param>
        /// <returns>Config object</returns>
        public static Config Read(string path)
        {
            Contract.Requires(path != null);
            Contract.Requires(path.Length > 0);

            Config cf;

            if (!File.Exists(path))
            {
                cf = new Config();
                cf.Write(ConfigPath);
                return cf;
            }

            cf = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
            return cf;
        }

        public void Write()
        {
            Write(ConfigPath);
        }

        /// <summary>
        /// Writes the configuration to a given path.
        /// </summary>
        /// <param name="path">string path - Location to put the config file</param>
        public void Write(string path)
        {
            Contract.Requires(path != null);
            Contract.Requires(path.Length > 0);

            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}