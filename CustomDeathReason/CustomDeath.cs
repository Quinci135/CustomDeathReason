﻿using System;
using System.IO.Streams;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using System.IO;
using Terraria.DataStructures;
using System.Text;

namespace CustomDeath
{
    [ApiVersion(2, 1)]
    public class CustomDeath : TerrariaPlugin
    {

        public override string Author => "Quinci";

        public override string Description => "Hardcoded custom death messages";

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
            if (args.MsgID == PacketTypes.PlayerDeathV2) //Checks the packet type for the playerdeath type, 118
            {
                using (MemoryStream data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
                {
                    // You can see how packets are handled in MessageBuffer.cs and how they are sent in NetMessage.cs
                    // TShock's GetDataHandler.cs can be used as an example on deserializing packets, same with other plugins
                    // Some custom types from terraria have their own methods to read from a binary reader (you can see this in use by tshock & terraria's messagebuffer)
                    // See https://tshock.readme.io/docs/multiplayer-packet-structure for more packet information
                    int player = data.ReadByte(); //Player ID
                    PlayerDeathReason deathReason = PlayerDeathReason.FromReader(new BinaryReader(data)); //Terraria.DataStructures.PlayerDeathReason
                    int damage = data.ReadInt16(); //Damage taken
                    int direction = data.ReadByte() - 1; //Direction, left or right, the player's body parts fly off to
                    bool pvp = ((BitsByte)data.ReadByte())[0];

                    args.Handled = true; //Prevents the game from processing this event

                    string customDeathReason = ""; //start of a custom death reason

                    Projectile proj = new Projectile(); //a projectile object used to get the name of a projectile given the projectile id/type
                    proj.SetDefaults(deathReason._sourceProjectileType);

                    if (pvp) //conditions could be cleaner
                    {
                        customDeathReason = (deathReason._sourcePlayerIndex == 0) ? $"[c/00ccff:{TShock.Players[player].Name}] was dealt [c/afef2a:{damage}] mortal damage by their own [i/p{deathReason._sourceItemPrefix}:{deathReason._sourceItemType}]" : $"[c/00ccff:{TShock.Players[player].Name}] was dealt [c/afef2a:{damage}] fatal damage by {TShock.Players[deathReason._sourcePlayerIndex].Name}'s [i/p{deathReason._sourceItemPrefix}:{deathReason._sourceItemType}]";
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

                    if (customDeathReason != "")
                    {
                        deathReason = PlayerDeathReason.ByCustomReason(customDeathReason);
                    }

                    Terraria.Main.player[player].KillMe(deathReason, damage, direction, pvp); //Terraria method used in MessageBuffer.cs (where packets are dealt with) to kill the player and send out the death reason to all clients

                }
            }
        }
    }
}
