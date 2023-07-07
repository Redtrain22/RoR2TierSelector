using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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

		/// <summary>
		/// This function will add *all items* in the items List to the GUI for editing.
		/// </summary>
		public void AddItemGUISettings()
		{
			ModSettingsManager.SetModDescription("Set custom item tiers");

			items.ForEach((item) =>
			{
				ModSettingsManager.AddOption(
					new IntSliderOption(item, new IntSliderConfig { min = (int)RoR2TierSelector.ItemTiers.Tier1, max = (int)RoR2TierSelector.ItemTiers.NoTier, restartRequired = true })
				);
			});
		}

		/// <summary>
		/// This function will add *a single equipment* in the equipments List to the GUI for editing.
		/// You must pass the EquipmentDef that you want to add to list to this function.
		/// </summary>
		public void AddEquipmentGUISetting(RoR2.EquipmentDef equipDef)
		{
			int index = equipments.FindIndex(configItem => (string)configItem.Definition.Key == equipDef.name);
			ConfigEntry<int> equipment = equipments.ElementAt(index);

			ModSettingsManager.AddOption(
				new IntSliderOption(equipment, new IntSliderConfig { min = (int)RoR2TierSelector.ItemTiers.Tier1, max = (int)RoR2TierSelector.ItemTiers.NoTier, restartRequired = true })
			);
		}

		public void ReloadConfig()
		{
			mainConfig.Reload();
		}

		// Not a fan of how long this is, but I don't see an easy way to reduce this to a more readable format.
		private static string itemHelpMessage = $@"{RoR2TierSelector.ItemTiers.Tier1} == {(int)RoR2TierSelector.ItemTiers.Tier1}, {RoR2TierSelector.ItemTiers.Tier2} == {(int)RoR2TierSelector.ItemTiers.Tier2}, {RoR2TierSelector.ItemTiers.Tier3} == {(int)RoR2TierSelector.ItemTiers.Tier3}
		{RoR2TierSelector.ItemTiers.Boss} == {(int)RoR2TierSelector.ItemTiers.Boss}, {RoR2TierSelector.ItemTiers.Lunar} == {(int)RoR2TierSelector.ItemTiers.Lunar}, {RoR2TierSelector.ItemTiers.NoTier} == {(int)RoR2TierSelector.ItemTiers.NoTier}";
		public void AddItemToList(List<ConfigEntry<int>> list, RoR2.ItemDef def)
		{
			items.Add(mainConfig.Bind<int>(new ConfigDefinition("Items", $"{def.name}"), (int)def.tier, new ConfigDescription($"{def.name} is {def.tier} by default.\n${itemHelpMessage}")));
		}

		private static string equipmentHelpMessage = $"None == 0, Standard == 1, Lunar == 2";
		public void AddEquipmentToList(List<ConfigEntry<int>> list, RoR2.EquipmentDef def)
		{
			equipments.Add(mainConfig.Bind<int>(new ConfigDefinition("Equipment", $"{def.name}"), def.isLunar ? 2 : 1, new ConfigDescription($"{def.name} is {(def.isLunar ? 2 : 1)} by default.\n{equipmentHelpMessage}")));
		}
	}
}