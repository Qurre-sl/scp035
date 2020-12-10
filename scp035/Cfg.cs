namespace scp035
{
    public class Cfg
    {
        public static ushort bct;
        public static string bc1;
        public static string bc2;
        public static string bc3;
        public static string cassie;
        public static string ra1;
        public static string ra2;
        public static string ra3;
        public static void Reload()
        {
            Cfg.bct = Plugin.Config.GetUShort("scp035_bc_time", 10);
            Cfg.bc1 = Plugin.Config.GetString("scp035_spawn_bc", $"<size=60>Вы-<color=red><b>SCP-035</b></color></size>\nВы заразили тело и получили контроль над ним, используйте его, чтобы помочь другим SCP!");
            Cfg.bc2 = Plugin.Config.GetString("scp035_damage_bc", "<size=25%><color=#6f6f6f>Вас атакует <color=red>SCP 035</color></color></size>");
            Cfg.bc3 = Plugin.Config.GetString("scp035_distance_bc", "<size=25%><color=#f47fff>*<color=#0089c7>принюхивается</color>*</color>\n<color=#6f6f6f>Вы чувствуете запах гнили, похоже это <color=red>SCP 035</color></color></size>");
            Cfg.cassie = Plugin.Config.GetString("scp035_cassie", "ATTENTION TO ALL PERSONNEL . SCP 0 3 5 ESCAPE . ALL HELICOPTERS AND MOBILE TASK FORCES IMMEDIATELY MOVE FORWARD TO GATE B . REPEAT ALL HELICOPTERS AND MOBILE TASK FORCES IMMEDIATELY MOVE FORWARD TO GATE B");
            Cfg.ra1 = Plugin.Config.GetString("scp035_command", "scp035");
            Cfg.ra2 = Plugin.Config.GetString("scp035_not_found", "Игрок не найден!");
            Cfg.ra3 = Plugin.Config.GetString("scp035_suc", "Успешно!");
        }
    }
}