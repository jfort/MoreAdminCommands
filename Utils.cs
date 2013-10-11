using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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


        #region SetUpConfig
        public static void SetUpConfig()
        {
            try
            {
                if (!File.Exists(MAC.savePath))
                {
                    MAC.config.Write(MAC.savePath);
                }
                else
                {
                    MAC.config = MACconfig.Read(MAC.savePath);
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid value in MoreAdminCommands.json");
                Console.ResetColor();
            }
        }
        #endregion
    }
}
