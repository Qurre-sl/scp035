using MEC;
using Mirror;
using Qurre.API;
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
			Map.ItemSpawn((ItemType)it, dur, m);
		}
		internal void KillScp035(bool setRank = true)
		{
			if (setRank)
				scpPlayer.SetRank("");
			scpPlayer.playerStats.maxHP = maxHP;
			scpPlayer = null;
			isRotating = true;
			RefreshItems();
		}
		public static void Spawn035(ReferenceHub p035)
		{
			maxHP = 300;
			p035.playerStats.maxHP = 300;
			p035.playerStats.Health = 300;
			p035.Broadcast(Cfg.bct, Cfg.bc1);
			Cassie.Send(Cfg.cassie, false, false);
			scpPlayer = p035;
			p035.SetRank("SCP 035", "red");
		}
		public void InfectPlayer(ReferenceHub player, Pickup pItem)
		{
			pItem.Delete();
			isRotating = false;
			Timing.CallDelayed(3f, () => Spawn035(player));
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
						IEnumerable<ReferenceHub> pList = Player.GetHubs().Where(x => x.queryProcessor.PlayerId != scpPlayer.queryProcessor.PlayerId);
						pList = pList.Where(x => x.GetTeam() != Team.SCP);
						pList = pList.Where(x => x.GetTeam() != Team.TUT);
						foreach (ReferenceHub player in pList)
						{
							if (player != null && Vector3.Distance(scpPlayer.transform.position, player.transform.position) <= 1.5f)
							{
								player.Broadcast(1, Cfg.bc2);
								CorrodePlayer(player);
							}
							else if (player != null && Vector3.Distance(scpPlayer.transform.position, player.transform.position) <= 15f)
								player.Broadcast(1, Cfg.bc3);

						}
					}
				}
				catch { }
			}
		}

		private void CorrodePlayer(ReferenceHub player)
		{
			if (scpPlayer != null)
			{
				int currHP = (int)scpPlayer.playerStats.Health;
				scpPlayer.playerStats.Health = currHP + 5 > 300 ? 300 : currHP + 5;
			}
			if (player.playerStats.Health - 5 > 0)
				player.playerStats.Health -= 5;
			else
			{
				player.Damage(55555, DamageTypes.None);
				ReferenceHub spy = scpPlayer;
				{
					Inventory.SyncListItemInfo items = new Inventory.SyncListItemInfo();
					foreach (var item in spy.inventory.items) items.Add(item);
					Vector3 pos1 = player.transform.position;
					Quaternion rot = spy.transform.rotation;
					int health = (int)spy.playerStats.Health;
					spy.SetRole(player.GetRole());
					Timing.CallDelayed(0.3f, () =>
					{
						spy.playerMovementSync.OverridePosition(pos1, 0f);
						spy.SetRotation(rot.x, rot.y);
						spy.inventory.items.Clear();
						foreach (var item in items) spy.inventory.AddNewItem(item.id);
						spy.playerStats.Health = health;
						spy.ammoBox.ResetAmmo();
					});
				}
				player.Damage(5, DamageTypes.Nuke);
				foreach (Ragdoll doll in UnityEngine.Object.FindObjectsOfType<Ragdoll>())
					if (doll.owner.PlayerId == player.queryProcessor.PlayerId)
						NetworkServer.Destroy(doll.gameObject);
			}
		}
	}
}