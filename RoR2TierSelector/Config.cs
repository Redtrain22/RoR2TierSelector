using BepInEx;
using BepInEx.Configuration;
using System.IO;
using System.Collections.Generic;

namespace RoR2TierSelector
{
	internal class ConfigManager
	{
		private static ConfigFile mainConfig;
		private static string ConfigName = $"{RoR2TierSelector.PluginName}.cfg";
		private static int configVersion = 1;
		private static string ConfigPath { get => Path.Combine(Paths.ConfigPath, ConfigName); }

		public struct Internals
		{
			public static int configVersion { get; set; }
		}

		public static List<ConfigEntry<int>> items = new List<ConfigEntry<int>>();
		
		public static List<ConfigEntry<int>> equipments = new List<ConfigEntry<int>>();

		public ConfigManager()
		{
			Init();
			LoadMainConfig();
		}

		private void Init()
		{
			mainConfig = new ConfigFile(ConfigPath, true);
		}

		private void LoadMainConfig()
		{
			Internals.configVersion = mainConfig.Bind<int>(
				new ConfigDefinition("Internals", "Version"),
				configVersion,
				new ConfigDescription("Internal Use to mark changes in config")
			).Value;
		}

		public void ReloadConfig()
		{
			mainConfig.Reload();
		}

		public void AddItemToList(List<ConfigEntry<int>> list, RoR2.ItemDef def)
		{
			items.Add(mainConfig.Bind<int>(new ConfigDefinition("Items", $"{def.name}"), (int)def.tier, new ConfigDescription( $"{def.name} is {def.tier} by default."
			+ "\n key : white = 0, green = 1, red = 2, lunar = 3, boss = 4, none = 5")));
		}

		public void AddEquipmentToList(List<ConfigEntry<int>> list, RoR2.EquipmentDef def) 
		{
			equipments.Add(mainConfig.Bind<int>(new ConfigDefinition("Equipment", $"{def.name}"), def.isLunar ? 2 : 1, new ConfigDescription($"{def.name} is {(def.isLunar ? 2 : 1)} by default."
			+ "\n key : none = 0, standard = 1, lunar = 2")));
		}
	}
}