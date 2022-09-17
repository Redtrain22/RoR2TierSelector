using BepInEx;
using BepInEx.Logging;
using RoR2;
using UnityEngine;

namespace RoR2TierSelector
{
	// Dependancies
	[BepInDependency(R2API.R2API.PluginGUID)]

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

		public void Awake()
		{
			Logger.Log(LogLevel.Info, $"Loaded {PluginName} v{PluginVersion}");
			config = new ConfigManager();
		}

		private void checkGameVersion(On.RoR2.RoR2Application.orig_Awake orig, RoR2Application self)
		{
			var buildId = Application.version;
			if (GameBuildId == buildId) return;

			Logger.LogWarning($"This version of \"{PluginName}\" was built for build id \"{GameBuildId}\", you are running \"{buildId}\".");
			Logger.LogWarning("Should any problems arise, please check for a new version before reporting issues.");

			orig(self);
		}
	}
}
