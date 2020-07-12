using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Streams;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using System.IO;
using Terraria.DataStructures;

namespace CustomDeath
{
    [ApiVersion(2, 1)]
    public class CustomDeath : TerrariaPlugin
    {

        public override string Author => "Quinci";

        public override string Description => "Read the name";

        public override string Name => "Custom Death messages";

        public override Version Version => new Version(1, 0, 0, 0);

        public CustomDeath(Main game) : base(game)
        {

        }

        public override void Initialize()
        {
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
            }
            base.Dispose(disposing);
        }

        private void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.PlayerDeathV2)
            {

                int player = args.Msg.readerStream.ReadByte();
                PlayerDeathReason deathReason = PlayerDeathReason.FromReader(new BinaryReader(args.Msg.readerStream));
                int damage = args.Msg.readerStream.ReadInt16();
                int direction = args.Msg.readerStream.ReadByte() - 1;
                bool pvp = ((BitsByte)args.Msg.readerStream.ReadByte())[0];
                args.Handled = true;
                TSPlayer.All.SendInfoMessage($"{TShock.Players[player].Name} has died");
                string customDeathReason = "yogurt";
                TSPlayer.All.SendInfoMessage($"damage: [c/afef2a:{damage}]");
                TSPlayer.All.SendInfoMessage($"deathReason._sourceItemType: {deathReason._sourceItemType}");
                TSPlayer.All.SendInfoMessage($"deathReason._sourceProjectileType: {deathReason._sourceProjectileType}");
                TSPlayer.All.SendInfoMessage($"deathReason._sourceProjectileIndex: {deathReason._sourceProjectileIndex}");
                TSPlayer.All.SendInfoMessage($"deathReason._sourceNPCIndex: {deathReason._sourceNPCIndex}");
                TSPlayer.All.SendInfoMessage($"deathReason._sourcePlayerIndex: {deathReason._sourcePlayerIndex}");
                TSPlayer.All.SendInfoMessage($"deathReason._sourceItemPrefix: {deathReason._sourceItemPrefix}");
                TSPlayer.All.SendInfoMessage($"deathReason._sourceOtherIndex: {deathReason._sourceOtherIndex}");
                TSPlayer.All.SendInfoMessage($"pvp: {pvp}");
                Projectile proj = new Projectile();
                proj.SetDefaults(deathReason._sourceProjectileType);

                if (pvp)
                {
                    customDeathReason = (deathReason._sourcePlayerIndex == 0) ? $"[c/00ccff:{TShock.Players[player].Name}] was dealt [c/afef2a:[c/afef2a:{damage}] mortal damage by their own [i/p{deathReason._sourceItemPrefix}:{deathReason._sourceItemType}]" : $"[c/00ccff:{TShock.Players[player].Name}] was dealt [c/afef2a:{damage}] fatal damage by {TShock.Players[deathReason._sourcePlayerIndex].Name}'s [i/p{deathReason._sourceItemPrefix}:{deathReason._sourceItemType}]";
                }
                else if (deathReason._sourceNPCIndex != -1)
                {
                    customDeathReason = $"[c/00ccff:{TShock.Players[player].Name}] was dealt [c/afef2a:{damage}] mortal damage by [c/00ff00:{Terraria.Main.npc[deathReason._sourceNPCIndex].TypeName}]";
                }
                else if (deathReason._sourceOtherIndex == -1 && proj.hostile)
                {
                    customDeathReason = $"[c/00ccff:{TShock.Players[player].Name}] was dealt [c/afef2a:{damage}] mortal damage by [c/00ff00:{proj.Name}]";
                }
                else if (deathReason._sourceOtherIndex == 4)
                {
                    string evil = WorldGen.crimson ? "Crimson" : "Demon";
                    customDeathReason = $"[c/00ccff:{TShock.Players[player].Name}] was driven insane trying to smash a [c/00ff00:{evil} altar]";
                }

                if (customDeathReason != "yogurt")
                {
                    deathReason = PlayerDeathReason.ByCustomReason(customDeathReason);
                }

                Terraria.Main.player[player].KillMe(deathReason, damage, direction, pvp);
            
            }
        }
    }
}