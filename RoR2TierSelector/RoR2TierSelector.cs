﻿using BepInEx;
using BepInEx.Logging;
using RoR2;
using R2API.Utils;
using UnityEngine;
using System.Linq;

using ItemCatalog = On.RoR2.ItemCatalog;

namespace RoR2TierSelector
{
	// Dependancies
	[BepInDependency(R2API.R2API.PluginGUID)]
	[R2APISubmoduleDependency(nameof(CommandHelper))]

	// MetaData for plugin
	[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
	public class RoR2TierSelector : BaseUnityPlugin
	{
		public const string PluginGUID = "me.redtrain.ror2tierselector";
		public const string PluginName = "RoR2TierSelector";
		// Note this project is also Authored by Heringfish02 just I don't know how to have multiple authors
		public const string PluginAuthor = "Redtrain22";
		public const string PluginVersion = "0.0.1";

		// Use for checking game version.
		private const string GameBuildId = "1.2.4.1";
		private static ConfigManager config;
		private enum ItemTiers
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

			// Hooks
			ItemCatalog.SetItemDefs += SetItemDefsHook;

			// Register Console Commands
			R2API.Utils.CommandHelper.AddToConsoleWhenReady();
		}

		private void checkGameVersion(On.RoR2.RoR2Application.orig_Awake orig, RoR2Application self)
		{
			var buildId = Application.version;
			if (GameBuildId == buildId) return;

			Logger.LogWarning($"This version of \"{PluginName}\" was built for build id \"{GameBuildId}\", you are running \"{buildId}\".");
			Logger.LogWarning("Should any problems arise, please check for a new version before reporting issues.");

			orig(self);
		}

		private void SetItemDefsHook(ItemCatalog.orig_SetItemDefs orig, ItemDef[] itemDefs)
		{
			foreach (var item in itemDefs)
			{
				config.AddItemToList(ConfigManager.items, item);
				int index = ConfigManager.items.FindIndex(configItem => (string)configItem.Definition.Key == item.name);
				ItemTier tier = (ItemTier)ConfigManager.items.ElementAt(index).Value;
				if (item.tier != tier)
				{
					item.tier = tier;
				}
			}

			orig.Invoke(itemDefs);
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
