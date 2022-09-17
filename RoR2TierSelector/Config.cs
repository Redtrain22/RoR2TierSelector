using BepInEx;
using BepInEx.Configuration;
using System.IO;

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

		public void reloadConfig()
		{
			mainConfig.Reload();
		}
	}
}