using MEC;
using Mirror;
using Qurre;
using Qurre.API;
using Qurre.API.Controllers.Items;
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
		private static List<Pickup> Items = new List<Pickup>();
		private const string TagForPlayer = " scp035";
		public void WFP() => Cfg.Reload();
		public void RoundStart()
		{
			RefreshItems();
			Timing.RunCoroutine(CorrodeUpdate(), "scp035coroutines");
		}
		public void RoundEnd(RoundEndEvent ev)
		{
			Timing.KillCoroutines("scp035coroutines");
		}
		public void PickupItem(PickupItemEvent ev)
		{
			if (Items.Contains(ev.Pickup) && ev.Pickup != null && ev.Allowed)
			{
				ev.Allowed = false;
				InfectPlayer(ev.Player, ev.Pickup);
			}
		}
		public void Damage(DamageEvent ev)
		{
			RemoveFF(ev.Attacker);
			if ((ev.Attacker.Tag.Contains(TagForPlayer) && ev.Target.Team == Team.SCP) ||
				(ev.Target.Tag.Contains(TagForPlayer) && ev.Attacker.Team == Team.SCP))
			{
				ev.Allowed = false;
				ev.Amount = 0f;
			}
			if (ev.Attacker.Id != ev.Target.Id &&
				((ev.Attacker.Tag.Contains(TagForPlayer) && ev.Target.Team == Team.TUT) ||
				(ev.Target.Tag.Contains(TagForPlayer) && ev.Attacker.Team == Team.TUT)))
			{
				ev.Allowed = false;
				ev.Amount = 0f;
			}
		}
		public void Shoot(ShootingEvent ev)
		{
			Player target = Player.List.ToList().Find(x => (x.Scale.x * 2) >= Vector3.Distance(x.Position, ev.Message.TargetPosition));
			if (target == null) return;
			if (target.Tag.Contains(TagForPlayer) || ev.Shooter.Tag.Contains(TagForPlayer))
				GrantFF(ev.Shooter);
		}
		public void Dies(DiesEvent ev)
		{
			if (ev.Killer.Tag.Contains(TagForPlayer) && ev.Killer?.Id != ev.Target?.Id)
			{
				if (ev.Target.Team == Team.SCP) return;
				if (ev.Target.Role == RoleType.Spectator) return;
				ev.Killer.ChangeBody(ev.Target.Role, true, ev.Target.Position, ev.Target.Rotation, ev.HitInfo.Tool);
			}
		}
		public void Dead(DeadEvent ev)
		{
			if (ev.Target.Tag.Contains(TagForPlayer))
				KillScp035(ev.Target);
			if (ev.Killer.Tag.Contains(TagForPlayer))
				foreach (Ragdoll doll in UnityEngine.Object.FindObjectsOfType<Ragdoll>())
					if (doll.owner.PlayerId == ev.Target.Id)
						NetworkServer.Destroy(doll.gameObject);
		}
		public void AddTarget(AddTargetEvent ev)
		{
			if (ev.Target.Tag.Contains(TagForPlayer))
				ev.Allowed = false;
		}
		public void PocketDimensionEnter(PocketEnterEvent ev)
		{
			if (ev.Player.Tag.Contains(TagForPlayer))
				ev.Allowed = false;
		}
		public void FemurBreaker(FemurBreakerEnterEvent ev)
		{
			if (ev.Player.Tag.Contains(TagForPlayer))
				ev.Allowed = false;
		}
		public void Escape(EscapeEvent ev)
		{
			if (ev.Player.Tag.Contains(TagForPlayer))
				ev.Allowed = false;
		}
		public void ChangeRole(RoleChangeEvent ev)
		{
			if (ev.Player.Tag.Contains(TagForPlayer) && ev.NewRole == RoleType.Spectator)
				KillScp035(ev.Player);
		}
		public void Leave(LeaveEvent ev)
		{
			if (ev.Player.Tag.Contains(TagForPlayer))
				KillScp035(ev.Player, true);
		}
		public void Contain(ContainEvent ev)
		{
			if (ev.Player.Tag.Contains(TagForPlayer))
				ev.Allowed = false;
		}
		public void Cuff(CuffEvent ev)
		{
			if (ev.Target.Tag.Contains(TagForPlayer))
				ev.Allowed = false;
		}
		public void Generator(InteractGeneratorEvent ev)
		{
			if (ev.Player.Tag.Contains(TagForPlayer))
				ev.Allowed = false;
		}
		public void Pocket(PocketFailEscapeEvent ev)
		{
			if (ev.Player.Tag.Contains(TagForPlayer))
			{
				ev.Allowed = false;
				ev.Player.TeleportTo106();
			}
		}
		public void Medical(ItemUsingEvent ev)
		{
			try
			{
				if (ev.Player == null) return;
				if (ev.Player.Tag.Contains(TagForPlayer))
				{
					ev.Player.MaxHp = 300;
				}
			}
			catch { }
		}
		public void Check(CheckEvent ev)
		{
			Player scp343ByKnuckles = null;
			try { scp343ByKnuckles = Player.List.Where(x => x.Tag.Contains("scp343-knuckles")).First(); } catch { }
			int mtf_team = ev.ClassList.mtf_and_guards + ev.ClassList.scientists;
			int d_team = ev.ClassList.chaos_insurgents + ev.ClassList.class_ds;
			int scp_team = ev.ClassList.scps_except_zombies + ev.ClassList.zombies;
			mtf_team -= Player.List.Where(x => x.Tag.Contains(TagForPlayer) && (x.Team == Team.MTF || x.Team == Team.RSC)).Count();
			d_team -= Player.List.Where(x => x.Tag.Contains(TagForPlayer) && (x.Team == Team.CDP || x.Team == Team.CHI)).Count();
			scp_team += Player.List.Where(x => x.Tag.Contains(TagForPlayer)).Count();
			if (scp343ByKnuckles != null)
			{
				if (scp343ByKnuckles.Team == Team.MTF || scp343ByKnuckles.Team == Team.RSC) mtf_team--;
				if (scp343ByKnuckles.Team == Team.CDP || scp343ByKnuckles.Team == Team.CHI) d_team--;
			}
			int count = 0;
			if (mtf_team > 0) ++count;
			if (d_team > 0) ++count;
			if (scp_team > 0) ++count;
			if (count <= 1) ev.RoundEnd = true;
		}
		public void Ra(SendingRAEvent ev)
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