using MEC;
using Mirror;
using Qurre;
using Qurre.API;
using Qurre.API.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace scp035
{
	public partial class EventHandlers
	{
		public static Plugin Plugin;
		public EventHandlers(Plugin plugin) => Plugin = plugin;
		private static Dictionary<Pickup, float> scpPickups = new Dictionary<Pickup, float>();
		internal static Player scpPlayer;
		internal bool isRoundStarted;
		internal static bool isRotating;
		private static int maxHP;
		private const float dur = 327;
		public static List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();
		public void WFP() => Cfg.Reload();
		public void RoundStart()
		{
			RefreshItems();
			isRoundStarted = true;
			isRotating = true;
			scpPlayer = null;
			Coroutines.Add(Timing.RunCoroutine(CorrodeUpdate()));
		}
		public void RoundEnd(RoundEndEvent ev)
		{
			isRoundStarted = false;
			Timing.KillCoroutines(Coroutines);
			Coroutines.Clear();
		}
		public void RoundRestart() => isRoundStarted = false;
		public void PickupItem(PickupItemEvent ev)
		{
			if (ev.Allowed)
				if (scpPlayer == null)
					if (ev.Pickup.durability == dur)
					{
						ev.Allowed = false;
						InfectPlayer(ev.Player, ev.Pickup);
					}
		}
		public void PlayerHurt(DamageEvent ev)
		{
			RemoveFF(ev.Attacker);
			if (scpPlayer != null)
			{
				if ((ev.Attacker.Id == scpPlayer?.Id &&
					ev.Target.Team == Team.SCP) ||
					(ev.Target.Id == scpPlayer?.Id &&
					ev.Attacker.Team == Team.SCP))
				{
					ev.Allowed = false;
					ev.Amount = 0f;
				}
				if (ev.Attacker.Id != ev.Target.Id &&
					((ev.Attacker.Id == scpPlayer?.Id &&
					ev.Target.Team == Team.TUT) ||
					(ev.Target.Id == scpPlayer?.Id &&
					ev.Attacker.Team == Team.TUT)))
				{
					ev.Allowed = false;
					ev.Amount = 0f;
				}
			}
		}
		public void Shoot(ShootingEvent ev)
		{
			if (ev.Target == null || scpPlayer == null) return;
			Player target = Player.Get(ev.Target);
			if (target == null) return;
			if (target.Id == scpPlayer?.Id || ev.Shooter.Id == scpPlayer?.Id)
				GrantFF(ev.Shooter);
		}
		public void PlayerDie(DiesEvent ev)
		{
			if (ev.Target.Id == scpPlayer?.Id)
				KillScp035();
			if (ev.Killer.Id == scpPlayer?.Id)
			{
				if (ev.Target.Team == Team.SCP) return;
				if (ev.Target.Role == RoleType.Spectator) return;
				Player spy = scpPlayer;
				{
					Inventory.SyncListItemInfo items = new Inventory.SyncListItemInfo();
					foreach (var item in spy.Inventory.items) items.Add(item);
					Vector3 pos1 = ev.Target.Position;
					Vector2 rot = spy.Rotations;
					int health = (int)spy.HP;
					spy.SetRole(ev.Target.Role);
					Timing.CallDelayed(0.3f, () =>
					{
						spy.Position = pos1;
						spy.Rotations = rot;
						spy.ClearInventory();
						foreach (var item in items) spy.AddItem(item.id);
						spy.HP = health;
						spy.Ammo.ResetAmmo();
					});
				}
			}
		}
		public void PlayerDied(DeadEvent ev)
		{
			if (ev.Killer.Id == scpPlayer?.Id)
				foreach (Ragdoll doll in UnityEngine.Object.FindObjectsOfType<Ragdoll>())
					if (doll.owner.PlayerId == ev.Target.Id)
						NetworkServer.Destroy(doll.gameObject);
		}
		public void scpzeroninesixe(EnrageEvent ev)
		{
			if (ev.Player.Id == scpPlayer?.Id)
			{
				ev.Allowed = false;
			}
		}
		public void scpzeroninesixeadd(AddTargetEvent ev)
		{
			if (ev.Target.Id == scpPlayer?.Id)
				ev.Allowed = false;
		}
		public void PocketDimensionEnter(PocketDimensionEnterEvent ev)
		{
			if (ev.Player.Id == scpPlayer?.Id)
				ev.Allowed = false;
		}
		public void FemurBreaker(FemurBreakerEnterEvent ev)
		{
			if (ev.Player.Id == scpPlayer?.Id)
				ev.Allowed = false;
		}
		public void CheckEscape(EscapeEvent ev)
		{
			if (ev.Player.Id == scpPlayer?.Id) ev.Allowed = false;
		}
		public void SetClass(RoleChangeEvent ev)
		{
			if (ev.Player.Id == scpPlayer?.Id)
				if (ev.NewRole == (global::RoleType)RoleType.Spectator)
					KillScp035();
		}
		public void PlayerLeave(LeaveEvent ev)
		{
			if (ev.Player.Id == scpPlayer?.Id)
				KillScp035(false);
		}
		public void Contain106(ContainEvent ev)
		{
			if (ev.Player.Id == scpPlayer?.Id)
				ev.Allowed = false;
		}
		public void PlayerHandcuffed(CuffEvent ev)
		{
			if (ev.Target.Id == scpPlayer?.Id)
				ev.Allowed = false;
		}
		public void InsertTablet(InteractGeneratorEvent ev)
		{
			if (ev.Player.Id == scpPlayer?.Id)
				ev.Allowed = false;
		}
		public void PocketDimensionDie(PocketDimensionFailEscapeEvent ev)
		{
			if (ev.Player.Id == scpPlayer?.Id)
			{
				ev.Allowed = false;
				Extensions.TeleportTo106(ev.Player);
			}
		}
		public void RunOnRACommandSent(SendingRAEvent ev)
		{
			try
			{
				var extractedArguments = ev.Command.Split(' ');
				string name = extractedArguments[0].ToLower();
				string[] args = extractedArguments.Skip(1).ToArray();
				List<string> arguments = args.ToList();
				string name1 = string.Join(" ", arguments.Skip(0));
				Player player = Player.Get(name1);
				if (name == Cfg.ra1)
				{
					ev.Allowed = false;
					if (player == null)
					{
						ev.ReplyMessage = Cfg.ra2;
						return;
					}
					ev.ReplyMessage = Cfg.ra3;
					Spawn035(player);
				}
			}
			catch (Exception e)
			{
				Log.Error("umm, error:\n" + e);
				ev.ReplyMessage = "umm, error:\n" + e;

			}
		}
		private void GrantFF(Player player) => player.FriendlyFire = true;
		private void RemoveFF(Player player) => player.FriendlyFire = false;
	}
}