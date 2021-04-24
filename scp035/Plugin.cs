using Qurre.Events;
namespace scp035
{
	public class Plugin : Qurre.Plugin
	{
		#region override
		public override System.Version Version => new System.Version(1, 0, 10);
		public override System.Version NeededQurreVersion => new System.Version(1, 3, 0);
		public override string Developer => "fydne";
		public override string Name => "scp035";
		public override int Priority => -10000;
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
			Round.WaitingForPlayers += EventHandlers.WFP;
			Round.Start += EventHandlers.RoundStart;
			Player.PickupItem += EventHandlers.PickupItem;
			Round.End += EventHandlers.RoundEnd;
			Round.Restart += EventHandlers.RoundRestart;
			Player.Dies += EventHandlers.PlayerDie;
			Player.Dead += EventHandlers.PlayerDied;
			Player.Damage += EventHandlers.PlayerHurt;
			SCPs.SCP106.PocketDimensionEnter += EventHandlers.PocketDimensionEnter;
			SCPs.SCP106.FemurBreakerEnter += EventHandlers.FemurBreaker;
			Player.Escape += EventHandlers.CheckEscape;
			Player.RoleChange += EventHandlers.SetClass;
			Player.Leave += EventHandlers.PlayerLeave;
			SCPs.SCP106.Contain += EventHandlers.Contain106;
			Player.Cuff += EventHandlers.PlayerHandcuffed;
			Player.InteractGenerator += EventHandlers.InsertTablet;
			SCPs.SCP106.PocketDimensionFailEscape += EventHandlers.PocketDimensionDie;
			Player.Shooting += EventHandlers.Shoot;
			Server.SendingRA += EventHandlers.RunOnRACommandSent;
			SCPs.SCP096.Enrage += EventHandlers.scpzeroninesixe;
			SCPs.SCP096.AddTarget += EventHandlers.scpzeroninesixeadd;
			Player.MedicalUsing += EventHandlers.Med;
			Round.Check += EventHandlers.Check;
		}
		public void UnregisterEvents()
		{
			Round.WaitingForPlayers -= EventHandlers.WFP;
			Round.Start -= EventHandlers.RoundStart;
			Player.PickupItem -= EventHandlers.PickupItem;
			Round.End -= EventHandlers.RoundEnd;
			Round.Restart -= EventHandlers.RoundRestart;
			Player.Dies -= EventHandlers.PlayerDie;
			Player.Dead -= EventHandlers.PlayerDied;
			Player.Damage -= EventHandlers.PlayerHurt;
			SCPs.SCP106.PocketDimensionEnter -= EventHandlers.PocketDimensionEnter;
			SCPs.SCP106.FemurBreakerEnter -= EventHandlers.FemurBreaker;
			Player.Escape -= EventHandlers.CheckEscape;
			Player.RoleChange -= EventHandlers.SetClass;
			Player.Leave -= EventHandlers.PlayerLeave;
			SCPs.SCP106.Contain -= EventHandlers.Contain106;
			Player.Cuff -= EventHandlers.PlayerHandcuffed;
			Player.InteractGenerator -= EventHandlers.InsertTablet;
			SCPs.SCP106.PocketDimensionFailEscape -= EventHandlers.PocketDimensionDie;
			Player.Shooting -= EventHandlers.Shoot;
			Server.SendingRA -= EventHandlers.RunOnRACommandSent;
			SCPs.SCP096.Enrage -= EventHandlers.scpzeroninesixe;
			SCPs.SCP096.AddTarget -= EventHandlers.scpzeroninesixeadd;
			Player.MedicalUsing -= EventHandlers.Med;
			Round.Check -= EventHandlers.Check;

			EventHandlers = null;
		}
		#endregion
	}
}