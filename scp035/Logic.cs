using MEC;
using Mirror;
using Qurre.API;
using Qurre.API.Controllers;
using Qurre.API.Controllers.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace scp035
{
	public partial class EventHandlers
	{
		private static void RemovePossessedItems()
		{
			foreach (Pickup p in Items.Where(x => x != null)) p.Destroy();
			Items.Clear();
		}
		internal void RefreshItems()
		{
			RemovePossessedItems();
			Vector3 m = Map.Rooms[Random.Range(0, Map.Rooms.Count - 1)].Position + Vector3.up;
			int it = Random.Range(0, 41);
			var p = new Item((ItemType)it).Spawn(m);
			if (p != null) Items.Add(p);
			else RefreshItems();
		}
		internal void KillScp035(Player pl, bool leave = false)
		{
			if (!leave)
			{
				pl.Tag = pl.Tag.Replace(TagForPlayer, "");
				pl.MaxHp = pl.ClassManager.CurRole.maxHP;
				pl.RoleColor = "default";
				pl.RoleName = "";
			}
			if (Player.List.Where(x => x.Tag.Contains(TagForPlayer)).Count() == 0)
				RefreshItems();
		}
		public static void Spawn035(Player p035)
		{
			p035.MaxHp = 300;
			p035.Hp = 300;
			p035.Broadcast(Cfg.bct, Cfg.bc1);
			Cassie.Send(Cfg.cassie, false, false, true);
			p035.RoleColor = "red";
			p035.RoleName = "SCP 035";
			p035.Tag += TagForPlayer;
		}
		public void InfectPlayer(Player player, Pickup pItem)
		{
			pItem.Destroy();
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
				int currHP = (int)scp035.Hp;
				scp035.Hp = currHP + 5 > 300 ? 300 : currHP + 5;
			}
			if (player.Hp - 5 > 0)
				player.Hp -= 5;
			else
			{
				scp035.ChangeBody(player.Role, true, player.Position, player.Rotation, DamageTypes.Falldown);
				player.Damage(55555, DamageTypes.Falldown);
				foreach (Ragdoll doll in UnityEngine.Object.FindObjectsOfType<Ragdoll>())
					if (doll.owner.PlayerId == player.Id)
						NetworkServer.Destroy(doll.gameObject);
			}
		}
	}
}