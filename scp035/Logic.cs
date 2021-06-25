using MEC;
using Mirror;
using Qurre.API;
using Qurre.API.Controllers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace scp035
{
	public partial class EventHandlers
	{
		private static void RemovePossessedItems()
		{
			for (int i = 0; i < scpPickups.Count; i++)
			{
				Pickup p = scpPickups.ElementAt(i).Key;
				if (p != null) p.Delete();
			}
			scpPickups.Clear();
		}
		internal void RefreshItems()
		{
			RemovePossessedItems();
			Vector3 m = Map.Rooms[Random.Range(0, Map.Rooms.Count - 1)].Position + Vector3.up;
			int it = Random.Range(0, 35);
			Qurre.API.Item.Spawn((ItemType)it, dur, m);
		}
		internal void KillScp035(bool setRank = true)
		{
			if (setRank) scpPlayer.ReferenceHub.SetRank("");
			scpPlayer.MaxHP = maxHP;
			scpPlayer = null;
			isRotating = true;
			RefreshItems();
		}
		public static void Spawn035(Player p035)
		{
			maxHP = 300;
			p035.MaxHP = 300;
			p035.HP = 300;
			p035.Broadcast(Cfg.bct, Cfg.bc1);
			Cassie.Send(Cfg.cassie, false, false, true);
			scpPlayer = p035;
			p035.ReferenceHub.SetRank("SCP 035", "red");
		}
		public void InfectPlayer(Player player, Pickup pItem)
		{
			pItem.Delete();
			isRotating = false;
			Spawn035(player);
			RemovePossessedItems();
		}
		internal IEnumerator<float> CorrodeUpdate()
		{
			for (; ; )
			{
				yield return Timing.WaitForSeconds(1f);
				try
				{
					if (scpPlayer != null && isRoundStarted)
					{
						IEnumerable<Player> pList = Player.List.Where(x => x.Id != scpPlayer.Id);
						pList = pList.Where(x => x.Team != Team.SCP);
						pList = pList.Where(x => x.Team != Team.TUT);
						pList = pList.Where(x => x.Team != Team.RIP);
						foreach (Player player in pList)
						{
							if (player != null && Vector3.Distance(scpPlayer.Position, player.Position) <= 1.5f)
							{
								player.Broadcast(1, Cfg.bc2);
								CorrodePlayer(player);
							}
							else if (player != null && Vector3.Distance(scpPlayer.Position, player.Position) <= 15f)
								player.Broadcast(1, Cfg.bc3);

						}
					}
				}
				catch { }
			}
		}

		private void CorrodePlayer(Player player)
		{
			if (scpPlayer != null)
			{
				int currHP = (int)scpPlayer.HP;
				scpPlayer.HP = currHP + 5 > 300 ? 300 : currHP + 5;
			}
			if (player.HP - 5 > 0)
				player.HP -= 5;
			else
			{
				scpPlayer.ChangeBody(player.Role, true, player.Position, player.Rotation, DamageTypes.Falldown);
				player.Damage(55555, DamageTypes.Falldown);
				foreach (Ragdoll doll in UnityEngine.Object.FindObjectsOfType<Ragdoll>())
					if (doll.owner.PlayerId == player.Id)
						NetworkServer.Destroy(doll.gameObject);
			}
		}
	}
}