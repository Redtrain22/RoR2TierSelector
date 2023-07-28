using BepInEx.Configuration;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine.Networking;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace RoR2TierSelector
{
	public class SyncConfig : INetMessage
	{
		NetworkInstanceId netId;
		private List<ConfigEntry<int>> items = new List<ConfigEntry<int>>();
		private List<ConfigEntry<int>> equipments = new List<ConfigEntry<int>>();

		public SyncConfig() { }

		public SyncConfig(NetworkInstanceId netId, List<ConfigEntry<int>> items, List<ConfigEntry<int>> equipments)
		{
			this.netId = netId;
			this.items = items;
			this.equipments = equipments;
		}
		public void Serialize(NetworkWriter writer)
		{
			// Create a MemoryStream to hold the serialized data
			using (MemoryStream stream = new MemoryStream())
			{
				// Serialize the configuration data to XML and write it to the MemoryStream
				XmlSerializer serializer = new XmlSerializer(typeof(List<ConfigEntry<int>>));
				serializer.Serialize(stream, netId);
				serializer.Serialize(stream, items);
				serializer.Serialize(stream, equipments);

				// Get the byte array representation of the serialized data
				byte[] data = stream.ToArray();

				// Write the byte array to the NetworkWriter
				writer.WriteBytesAndSize(data, data.Length);
			}
		}
		public void Deserialize(NetworkReader reader)
		{
			// Read the byte array from the NetworkReader
			byte[] data = reader.ReadBytesAndSize();

			// Create a MemoryStream from the byte array
			using (MemoryStream stream = new MemoryStream(data))
			{
				// Deserialize the configuration data from the MemoryStream
				XmlSerializer serializer = new XmlSerializer(typeof(List<ConfigEntry<int>>));
				netId = (NetworkInstanceId)serializer.Deserialize(stream);
				items = (List<ConfigEntry<int>>)serializer.Deserialize(stream);
				equipments = (List<ConfigEntry<int>>)serializer.Deserialize(stream);
			}
		}

		public void OnReceived()
		{
			if (!NetworkServer.active)
			{
				return;
			}
			Chat.AddMessage($"Client received SyncConfig.");

			// Update the items and equipments lists in the ConfigManager with the received lists
			ConfigManager.items = this.items;
			ConfigManager.equipments = this.equipments;
			RoR2TierSelector.ReloadItemTiers();
		}

	}
}