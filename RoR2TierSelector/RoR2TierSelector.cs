using BepInEx;

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

		public void Awake()
		{
			Logger.Log(BepInEx.Logging.LogLevel.Info, "Plugin Loaded");
		}
	}
}
