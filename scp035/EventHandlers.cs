using MEC;
using Mirror;
using Qurre;
using Qurre.API;
using Qurre.API.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Qurre.API.Events.SCP096;
using static Qurre.API.Events.SCP106;
namespace scp035
{
	public partial class EventHandlers
	{
		public static Plugin Plugin;
		public EventHandlers(Plugin plugin) => Plugin = plugin;
		private static Dictionary<Pickup, float> scpPickups = new Dictionary<Pickup, float>();
		internal static ReferenceHub scpPlayer;
		internal bool isRoundStarted;
		internal static bool isRotating;
		private static int maxHP;
		private const float dur = 327;
		public static List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();
		public void WFP() => Cfg.Reload();
		public void RoundStart()
		{
			isRoundStarted = true;
			isRotating = true;
			scpPickups.Clear();
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
			if (ev.IsAllowed)
				if (scpPlayer == null)
					if (ev.Pickup.durability == dur)
					{
						ev.IsAllowed = false;
						InfectPlayer(ev.Player, ev.Pickup);
					}
		}
		public void PlayerHurt(DamageEvent ev)
		{
			RemoveFF(ev.Attacker);
			if (scpPlayer != null)
			{
				if ((ev.Attacker.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId &&
					ev.Target.GetTeam() == Team.SCP) ||
					(ev.Target.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId &&
					ev.Attacker.GetTeam() == Team.SCP))
				{
					ev.IsAllowed = false;
					ev.Amount = 0f;
				}
				if (ev.Attacker.queryProcessor.PlayerId != ev.Target.queryProcessor.PlayerId &&
					((ev.Attacker.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId &&
					ev.Target.GetTeam() == Team.TUT) ||
					(ev.Target.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId &&
					ev.Attacker.GetTeam() == Team.TUT)))
				{
					ev.IsAllowed = false;
					ev.Amount = 0f;
				}
			}
		}
		public void Shoot(ShootingEvent ev)
		{
			if (ev.Target == null || scpPlayer == null) return;
			ReferenceHub target = Player.Get(ev.Target);
			if (target == null) return;
			if ((ev.Shooter.GetPlayerId() == scpPlayer?.queryProcessor.PlayerId &&
				target.GetTeam() == scpPlayer?.GetTeam())
				|| (target.GetPlayerId() == scpPlayer?.queryProcessor.PlayerId &&
				ev.Shooter.GetTeam() == scpPlayer?.GetTeam()))
				GrantFF(ev.Shooter);
			if ((ev.Shooter.GetPlayerId() == scpPlayer?.queryProcessor.PlayerId || target.GetPlayerId() == scpPlayer?.queryProcessor.PlayerId) &&
				(((ev.Shooter.GetTeam() == Team.CDP && target.GetTeam() == Team.CHI)
				|| (ev.Shooter.GetTeam() == Team.CHI && target.GetTeam() == Team.CDP)) ||
				(ev.Shooter.GetTeam() == Team.RSC && target.GetTeam() == Team.MTF)
				|| (ev.Shooter.GetTeam() == Team.MTF && target.GetTeam() == Team.RSC)))
				GrantFF(ev.Shooter);
		}
		public void PlayerDie(DiedEvent ev)
		{
			if (ev.Target.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
				KillScp035();
			if (ev.Killer.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
			{
				if (ev.Target.GetTeam() == Team.SCP) return;
				if (ev.Target.GetRole() == RoleType.Spectator) return;
				ReferenceHub spy = scpPlayer;
				{
					Inventory.SyncListItemInfo items = new Inventory.SyncListItemInfo();
					foreach (var item in spy.inventory.items) items.Add(item);
					Vector3 pos1 = ev.Target.transform.position;
					Quaternion rot = spy.transform.rotation;
					int health = (int)spy.playerStats.Health;
					spy.SetRole(ev.Target.GetRole());
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
			}
		}
		public void PlayerDied(DiedEvent ev)
		{
			if (ev.Killer.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
				foreach (Ragdoll doll in UnityEngine.Object.FindObjectsOfType<Ragdoll>())
					if (doll.owner.PlayerId == ev.Target.queryProcessor.PlayerId)
						NetworkServer.Destroy(doll.gameObject);
		}
		public void scpzeroninesixe(EnrageEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
			{
				ev.IsAllowed = false;
			}
		}
		public void scpzeroninesixeadd(AddTargetEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
				ev.IsAllowed = false;
		}
		public void PocketDimensionEnter(PocketDimensionEnterEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
				ev.IsAllowed = false;
		}
		public void FemurBreaker(FemurBreakerEnterEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
				ev.IsAllowed = false;
		}
		public void CheckEscape(EscapeEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId) ev.IsAllowed = false;
		}
		public void SetClass(RoleChangeEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
				if (ev.NewRole == (global::RoleType)RoleType.Spectator)
					KillScp035();
		}
		public void PlayerLeave(LeaveEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
				KillScp035(false);
		}
		public void Contain106(ContainEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
				ev.IsAllowed = false;
		}
		public void PlayerHandcuffed(CuffEvent ev)
		{
			if (ev.Target.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
				ev.IsAllowed = false;
		}
		public void InsertTablet(InteractGeneratorEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
				ev.IsAllowed = false;
		}
		public void PocketDimensionDie(PocketDimensionFailEscapeEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
			{
				ev.IsAllowed = false;
				Extensions.TeleportTo106(ev.Player);
			}
		}
		public void RunOnRACommandSent(SendingRAEvent ev)
		{
			try
			{
				var extractedArguments = ev.Message.Split(' ');
				string name = extractedArguments[0].ToLower();
				string[] args = extractedArguments.Skip(1).ToArray();
				List<string> arguments = args.ToList();
				string name1 = string.Join(" ", arguments.Skip(0));
				ReferenceHub player = Player.Get(name1);
				if (name == Cfg.ra1)
				{
					ev.IsAllowed = false;
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
		private void GrantFF(ReferenceHub player) => player.SetFriendlyFire(true);
		private void RemoveFF(ReferenceHub player) => player.SetFriendlyFire(false);
	}
}