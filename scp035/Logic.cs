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
			foreach (Pickup p in Map.Pickups.Where(x => x.durability == dur && x != null)) p.Delete();
		}
		internal void RefreshItems()
		{
			RemovePossessedItems();
			Vector3 m = Map.Rooms[Random.Range(0, Map.Rooms.Count - 1)].Position + Vector3.up;
			int it = Random.Range(0, 35);
			Qurre.API.Item.Spawn((ItemType)it, dur, m);
		}
		internal void KillScp035(Player pl, bool leave = false)
		{
			if (!leave)
			{
				pl.Tag = pl.Tag.Replace(TagForPlayer, "");
				pl.MaxHP = pl.ClassManager.CurRole.maxHP;
				pl.RoleColor = "default";
				pl.RoleName = "";
			}
			if (Player.List.Where(x => x.Tag.Contains(TagForPlayer)).Count() == 0)
				RefreshItems();
		}
		public static void Spawn035(Player p035)
		{
			p035.MaxHP = 300;
			p035.HP = 300;
			p035.Broadcast(Cfg.bct, Cfg.bc1);
			Cassie.Send(Cfg.cassie, false, false, true);
			p035.RoleColor = "red";
			p035.RoleName = "SCP 035";
			p035.Tag += TagForPlayer;
		}
		public void InfectPlayer(Player player, Pickup pItem)
		{
			pItem.Delete();
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
					if (Round.Started)
					{
						IEnumerable<Player> pList = Player.List.Where(x => !x.Tag.Contains(TagForPlayer));
						pList = pList.Where(x => x.Team != Team.SCP);
						pList = pList.Where(x => x.Team != Team.TUT);
						pList = pList.Where(x => x.Team != Team.RIP);
						foreach (Player scp035 in Player.List.Where(x => x.Tag.Contains(TagForPlayer)))
						{
							foreach (Player player in pList)
							{
								if (player != null && Vector3.Distance(scp035.Position, player.Position) <= 1.5f)
								{
									player.Broadcast(1, Cfg.bc2);
									CorrodePlayer(player, scp035);
								}
								else if (player != null && Vector3.Distance(scp035.Position, player.Position) <= 15f)
									player.Broadcast(1, Cfg.bc3);
							}
						}
					}
				}
				catch { }
			}
		}

		private void CorrodePlayer(Player player, Player scp035)
		{
			if (scp035 != null)
			{
				int currHP = (int)scp035.HP;
				scp035.HP = currHP + 5 > 300 ? 300 : currHP + 5;
			}
			if (player.HP - 5 > 0)
				player.HP -= 5;
			else
			{
				scp035.ChangeBody(player.Role, true, player.Position, player.Rotation, DamageTypes.Falldown);
				Timing.CallDelayed(2f, () =>
				{
					scp035.RoleColor = "red";
					scp035.RoleName = "SCP 035";
				});
				player.Damage(55555, DamageTypes.Falldown);
				foreach (Ragdoll doll in UnityEngine.Object.FindObjectsOfType<Ragdoll>())
					if (doll.owner.PlayerId == player.Id)
						NetworkServer.Destroy(doll.gameObject);
			}
		}
	}
}