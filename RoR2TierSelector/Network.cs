using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace RoR2TierSelector
{
	public class SyncConfig : INetMessage
	{
		NetworkInstanceId netId;

		public SyncConfig()
		{
		}

		public SyncConfig(NetworkInstanceId netId)
		{

		}

		public void Deserialize(NetworkReader reader)
		{

		}

		public void OnReceived()
		{
			UnityEngine.Debug.Log("Attempting to sync full config list between client and server.");
			if (NetworkServer.active) return;
		}

		public void Serialize(NetworkWriter writer)
		{

		}
	}
}