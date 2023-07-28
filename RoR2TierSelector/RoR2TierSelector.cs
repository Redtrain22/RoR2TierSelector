using BepInEx;
using BepInEx.Logging;
using RoR2;
using R2API.Utils;
using UnityEngine;
using System;
using System.Linq;
using R2API.Networking;
using UnityEngine.Networking;
using System.Reflection;
using System.Collections.Generic;

using ItemCatalog = On.RoR2.ItemCatalog;
using EquipmentCatalog = On.RoR2.EquipmentCatalog;

// New way to register commands
[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace RoR2TierSelector
{
	// Dependencies
	// This is a solid mod choice to use to create a menu in the game to change item values
	// [BepInDependency("com.rune580.riskofoptions")]
	[BepInDependency(R2API.R2API.PluginGUID)]

	// MetaData for plugin
	[BepInPlugin(PluginGUID, PluginName, PluginVersion)]

	// NetworkingAPI for plugin
	[BepInDependency(NetworkingAPI.PluginGUID)]

	// Network Compatibility
	[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
	public class RoR2TierSelector : BaseUnityPlugin
	{
		public const string PluginGUID = "me.redtrain.ror2tierselector";
		public const string PluginName = "RoR2TierSelector";
		// Note this project is also authored by Redtrain22 and Heringfish02.
		// The plugin author is thus a combination of the two usernames.
		public const string PluginAuthor = "RedHering";
		public const string PluginVersion = "0.0.1";

		// Use for checking game version.
		private const string GameBuildId = "1.2.4.1";
		internal static ConfigManager config;
		internal enum ItemTiers
		{
			Tier1,
			Tier2,
			Tier3,
			Lunar,
			Boss,
			NoTier,
			VoidTier1,
			VoidTier2,
			VoidTier3,
			VoidBoss,
			AssignedAtRuntime
		}

		public void Awake()
		{
			Logger.Log(LogLevel.Info, $"Loaded {PluginName} v{PluginVersion}");
			config = new ConfigManager();

			NetworkingAPI.RegisterMessageType<SyncConfig>();

			// Hooks
			EquipmentCatalog.RegisterEquipment += RegisterEquipmentHook;
			ItemCatalog.SetItemDefs += SetItemDefsHook;
		}

		private void checkGameVersion(On.RoR2.RoR2Application.orig_Awake orig, RoR2Application self)
		{
			var buildId = Application.version;
			if (GameBuildId == buildId) return;

			Logger.LogWarning($"This version of \"{PluginName}\" was built for build id \"{GameBuildId}\", you are running \"{buildId}\".");
			Logger.LogWarning("Should any problems arise, please check for a new version before reporting issues.");

			orig(self);
		}

		private static void LogRoR2Assemblies()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (Assembly assembly in assemblies)
			{
				if (!assembly.FullName.StartsWith("RoR2") | !assembly.FullName.StartsWith("RoR2.Items"))
					continue;

				Type[] types = assembly.GetTypes();

				Debug.Log($"Types in assembly '{assembly.FullName}':");
				foreach (Type type in types)
				{
					Debug.Log(type.FullName);
				}
			}
		}
		 public static void LogAvailableFields()
    {
        Type itemCatalogType = typeof(RoR2.ItemCatalog);
        FieldInfo[] fields = itemCatalogType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        Debug.Log("Available Fields in ItemCatalog:");

        foreach (FieldInfo field in fields)
        {
            Debug.Log("Field Name: " + field.Name);
            Debug.Log("Declaring Type: " + field.DeclaringType);
            Debug.Log("Field Type: " + field.FieldType);
            Debug.Log("-------------------------");
        }
    }

		internal static void ReloadItemTiers()
		{
			// Re-Initialize the item and equipment Defs, and Display Rules to work with Command / Log Book
			typeof(RoR2.ItemCatalog).GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
			typeof(RoR2.ItemDisplayRuleSet).GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
			typeof(RoR2.MiscPickupCatalog).GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
			typeof(RoR2.PickupCatalog).GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
			typeof(RoR2.UI.ItemInventoryDisplay).GetMethod("ResetItems", BindingFlags.Public)?.Invoke(null, null);
			typeof(RoR2.UI.ItemInventoryDisplay).GetMethod("UpdateDisplay", BindingFlags.Public)?.Invoke(null, null);
			typeof(RoR2.UI.LogBook.LogBookController).GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
		}
		private void SetItemDefsHook(ItemCatalog.orig_SetItemDefs orig, ItemDef[] itemDefs)
		{
			foreach (ItemDef item in itemDefs)
			{
				config.AddItemToList(ConfigManager.items, item);
				int index = ConfigManager.items.FindIndex(configItem => (string)configItem.Definition.Key == item.name);
				ItemTier tier = (ItemTier)ConfigManager.items.ElementAt(index).Value;
				if (item.tier != tier)
				{
					item.tier = tier;
				}
			}

			config.AddItemGUISettings();
			orig.Invoke(itemDefs);
			// Logger.Log(LogLevel.Debug, "Items are changing");
		}

		private void RegisterEquipmentHook(EquipmentCatalog.orig_RegisterEquipment orig, EquipmentIndex equipmentIndex, EquipmentDef equipDef)
		{
			config.AddEquipmentToList(ConfigManager.equipments, equipDef);
			int index = ConfigManager.equipments.FindIndex(configItem => (string)configItem.Definition.Key == equipDef.name);
			int tier = equipDef.isLunar ? 2 : 1;
			switch (ConfigManager.equipments.ElementAt(index).Value)
			{
				case 1:
					equipDef.isLunar = false;
					equipDef.colorIndex = ColorCatalog.ColorIndex.Equipment;
					break;
				case 2:
					equipDef.isLunar = true;
					equipDef.colorIndex = ColorCatalog.ColorIndex.LunarItem;
					break;
				default:
					equipDef.canDrop = false;
					equipDef.isLunar = false; // Don't know if this is needed, but better safe than sorry!
					break;
			}

			config.AddEquipmentGUISetting(equipDef);
			orig.Invoke(equipmentIndex, equipDef);
		}

		[ConCommand(commandName = "set_item_tier", flags = RoR2.ConVarFlags.Engine, helpText = "Sets an item tier.")]
		private static void ccSetItemTier(RoR2.ConCommandArgs args)
		{
			args.CheckArgumentCount(2);

			string itemName = args.TryGetArgString(0);
			int newTier = (int)args.TryGetArgInt(1);
			int index = ConfigManager.items.FindIndex(configItem => configItem.Definition.Key.ToLower() == itemName.ToLower());
			ConfigManager.items.ElementAt(index).Value = newTier;

			// Can't hot reload the item defs without something like reflection.
			// TODO Load the changed tiers into the game???
			// ReloadItemTiers();

			UnityEngine.Debug.Log($"{ConfigManager.items.ElementAt(index).Definition.Key} is now set to Tier {newTier} in the config, please restart your game for it to take effect.");
		}

		[ConCommand(commandName = "get_item_tier", flags = RoR2.ConVarFlags.Engine, helpText = "Gets an item tier.")]
		private static void ccGetItemTier(RoR2.ConCommandArgs args)
		{
			args.CheckArgumentCount(1);

			string itemName = args.TryGetArgString(0);
			int index = ConfigManager.items.FindIndex(configItem => configItem.Definition.Key.ToLower() == itemName.ToLower());

			UnityEngine.Debug.Log((int)ConfigManager.items.ElementAt(index).Value);
		}
	}
}
