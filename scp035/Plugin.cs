using Qurre.Events;
namespace scp035
{
	public class Plugin : Qurre.Plugin
	{
		#region override
		public override System.Version Version => new System.Version(1, 1, 6);
		public override System.Version NeededQurreVersion => new System.Version(1, 9, 0);
		public override string Developer => "fydne";
		public override string Name => "scp035";
		public override int Priority => 10000;
		public override void Enable() => RegisterEvents();
		public override void Disable() => UnregisterEvents();
		public override void Reload() => Cfg.Reload();
		#endregion
		#region Events
		internal EventHandlers EventHandlers;
		public void RegisterEvents()
		{
			Cfg.Reload();
			EventHandlers = new EventHandlers(this);
			Round.Waiting += EventHandlers.WFP;
			Round.Start += EventHandlers.RoundStart;
			Player.PickupItem += EventHandlers.PickupItem;
			Round.End += EventHandlers.RoundEnd;
			Player.Dies += EventHandlers.Dies;
			Player.Dead += EventHandlers.Dead;
			Player.Damage += EventHandlers.Damage;
			Scp106.PocketEnter += EventHandlers.PocketDimensionEnter;
			Scp106.FemurBreakerEnter += EventHandlers.FemurBreaker;
			Player.Escape += EventHandlers.Escape;
			Player.RoleChange += EventHandlers.ChangeRole;
			Player.Leave += EventHandlers.Leave;
			Scp106.Contain += EventHandlers.Contain;
			Player.Cuff += EventHandlers.Cuff;
			Player.InteractGenerator += EventHandlers.Generator;
			Scp106.PocketFailEscape += EventHandlers.Pocket;
			Player.Shooting += EventHandlers.Shoot;
			Server.SendingRA += EventHandlers.Ra;
			Scp096.AddTarget += EventHandlers.AddTarget;
			Player.ItemUsing += EventHandlers.Medical;
			Round.Check += EventHandlers.Check;
		}
		public void UnregisterEvents()
		{
			Round.Waiting -= EventHandlers.WFP;
			Round.Start -= EventHandlers.RoundStart;
			Player.PickupItem -= EventHandlers.PickupItem;
			Round.End -= EventHandlers.RoundEnd;
			Player.Dies -= EventHandlers.Dies;
			Player.Dead -= EventHandlers.Dead;
			Player.Damage -= EventHandlers.Damage;
			Scp106.PocketEnter -= EventHandlers.PocketDimensionEnter;
			Scp106.FemurBreakerEnter -= EventHandlers.FemurBreaker;
			Player.Escape -= EventHandlers.Escape;
			Player.RoleChange -= EventHandlers.ChangeRole;
			Player.Leave -= EventHandlers.Leave;
			Scp106.Contain -= EventHandlers.Contain;
			Player.Cuff -= EventHandlers.Cuff;
			Player.InteractGenerator -= EventHandlers.Generator;
			Scp106.PocketFailEscape -= EventHandlers.Pocket;
			Player.Shooting -= EventHandlers.Shoot;
			Server.SendingRA -= EventHandlers.Ra;
			Scp096.AddTarget -= EventHandlers.AddTarget;
			Player.ItemUsing -= EventHandlers.Medical;
			Round.Check -= EventHandlers.Check;

			EventHandlers = null;
		}
		#endregion
	}
}