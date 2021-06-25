using MEC;
using Mirror;
using Qurre;
using Qurre.API;
using Qurre.API.Events;
using System;
using System.Collections.Generic;
using System.Linq;
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
		public void WFP() => Cfg.Reload();
		public void RoundStart()
		{
			RefreshItems();
			isRoundStarted = true;
			isRotating = true;
			scpPlayer = null;
			Timing.RunCoroutine(CorrodeUpdate(), "scp035coroutines");
		}
		public void RoundEnd(RoundEndEvent ev)
		{
			isRoundStarted = false;
			Timing.KillCoroutines("scp035coroutines");
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
			{
				KillScp035();
				return;
			}
			if (ev.Killer?.Id == scpPlayer?.Id && ev.Killer?.Id != ev.Target?.Id)
			{
				if (ev.Target.Team == Team.SCP) return;
				if (ev.Target.Role == RoleType.Spectator) return;
				scpPlayer.ChangeBody(ev.Target.Role, true, ev.Target.Position, ev.Target.Rotation, ev.HitInfo.GetDamageType());
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
			if (ev.Player.Id == scpPlayer?.Id) ev.Allowed = false;
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
			if (ev.Player.Id == scpPlayer?.Id)
				ev.Allowed = false;
		}
		public void SetClass(RoleChangeEvent ev)
		{
			if (ev.Player.Id == scpPlayer?.Id && ev.NewRole == RoleType.Spectator)
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
		public void Med(MedicalUsingEvent ev)
		{
			try
			{
				if (ev.Player == null || ev.Player.UserId == null || scpPlayer == null || scpPlayer.UserId == null) return;
				if (ev.Player.UserId == scpPlayer.UserId)
				{
					ev.Player.MaxHP = 300;
				}
			}
			catch { }
		}
		public void Check(CheckEvent ev)
		{
			if (scpPlayer == null || scpPlayer.UserId == null) return;
			int mtf_team = ev.ClassList.mtf_and_guards + ev.ClassList.scientists;
			int d_team = ev.ClassList.chaos_insurgents + ev.ClassList.class_ds;
			int scp_team = ev.ClassList.scps_except_zombies + ev.ClassList.zombies;
			if (scpPlayer.Team == Team.MTF || scpPlayer.Team == Team.RIP) mtf_team--;
			else if (scpPlayer.Team == Team.CDP || scpPlayer.Team == Team.CHI) d_team--;
			scp_team++;
			int count = 0;
			if (mtf_team > 0) ++count;
			if (d_team > 0) ++count;
			if (scp_team > 0) ++count;
			if (count <= 1) Round.End();
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