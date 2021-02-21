using Interactables.Interobjects.DoorUtils;
using Qurre.API;
using System.Linq;
using UnityEngine;
namespace scp035
{
	public static class Extensions
	{
		public static void SetRank(this ReferenceHub player, string rank, string color = "default")
		{
			player.serverRoles.NetworkMyText = rank;
			player.serverRoles.NetworkMyColor = color;
		}
		internal static void TeleportTo106(Player player)
		{
			try
			{
				Player scp106 = Player.List.Where(x => x.Role == RoleType.Scp106).FirstOrDefault();
				Vector3 toded = scp106.Position;
				player.Position = toded;
			}
			catch
			{
				player.Position = Map.GetRandomSpawnPoint(RoleType.Scp096);
			}
		}
	}
}