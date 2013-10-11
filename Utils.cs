using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoreAdminCommands
{
    public class Utils
    {
        public static Mplayer GetPlayers(string name)
        {
            foreach (Mplayer player in MAC.Players)
            {
                if (player.name.ToLower() == name.ToLower())
                {
                    return player;
                }
            }
            return null;
        }

        public static Mplayer GetPlayers(int index)
        {
            foreach (Mplayer player in MAC.Players)
            {
                if (player.Index == index)
                {
                    return player;
                }
            }
            return null;
        }
    }
}
