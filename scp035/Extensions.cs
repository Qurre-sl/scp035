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
		internal static void TeleportTo106(ReferenceHub player)
		{
			try
			{
				ReferenceHub scp106 = Player.GetHubs().Where(x => x.characterClassManager.CurClass == RoleType.Scp106).FirstOrDefault();
				Vector3 toded = scp106.transform.position;
				player.SetPosition(toded);
			}
			catch
			{
				player.SetPosition(Map.GetRandomSpawnPoint(RoleType.Scp096));
			}
		}
	}
}