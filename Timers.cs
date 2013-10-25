using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

using Terraria;
using TShockAPI;

namespace MoreAdminCommands
{
    public class updateTimers
    {
        public static Timer timeTimer = new Timer(1000);
        public static Timer autoKillTimer = new Timer(1000);
        public static Timer viewAllTimer = new Timer(1000);
        public static Timer permaBuffTimer = new Timer(1000);
        public static Timer permaDebuffTimer = new Timer(1000);
        public static Timer disableTimer = new Timer(1000);

        #region PermaBuffTimer
        public static void startPermaBuffTimer()
        {
            permaBuffTimer.Enabled = true;
            permaBuffTimer.Elapsed += new ElapsedEventHandler(pBTimer);
        }

        public static void stopPermaBuffTimer()
        {
            permaBuffTimer.Enabled = false;
            permaBuffTimer.Elapsed -= new ElapsedEventHandler(pBTimer);
        }

        public static void pBTimer(object sender, ElapsedEventArgs args)
        {
            int count = 0;
            foreach (Mplayer player in MAC.Players)
            {
                if (player.isPermabuff)
                {
                    foreach (int activeBuff in player.TSPlayer.TPlayer.buffType)
                    {
                        if (!Main.debuff[activeBuff])
                        {
                            player.TSPlayer.SetBuff(activeBuff, Int16.MaxValue);
                        }
                    }
                    count++;
                }
            }
            if (count == 0)
                stopPermaBuffTimer();

            count = 0;
        }
        #endregion

        #region DisableTimer
        public static void startDisableTimer()
        {
            disableTimer.Enabled = true;
            disableTimer.Elapsed += new ElapsedEventHandler(dTimer);
        }

        public static void stopDisableTimer()
        {
            disableTimer.Enabled = false;
            disableTimer.Elapsed -= new ElapsedEventHandler(dTimer);
        }

        public static void dTimer(object sender, ElapsedEventArgs args)
        {
            int count = 0;
            foreach (Mplayer player in MAC.Players)
            {
                if (player.isDisabled)
                {
                    player.TSPlayer.SetBuff(47, 180);
                    count++;
                }
            }

            if (count == 0)
                stopDisableTimer();

            count = 0;
        }
        #endregion

        #region PermaDebuffTimer
        public static void startPermaDebuffTimer()
        {
            permaDebuffTimer.Enabled = true;
            permaDebuffTimer.Elapsed += new ElapsedEventHandler(pDTimer);
        }

        public static void stopPermaDebuffTimer()
        {
            permaDebuffTimer.Enabled = false;
            permaDebuffTimer.Elapsed -= new ElapsedEventHandler(pDTimer);
        }

        public static void pDTimer(object sender, ElapsedEventArgs args)
        {
            int count = 0;
            foreach (Mplayer player in MAC.Players)
            {
                if (player.isPermaDebuff)
                {
                    foreach (int activeBuff in player.TSPlayer.TPlayer.buffType)
                    {
                        if (Main.debuff[activeBuff])
                        {
                            player.TSPlayer.SetBuff(activeBuff, Int16.MaxValue);
                        }
                    }
                    count++;
                }
            }
            if (count == 0)
                stopPermaDebuffTimer();

            count = 0;
        }
        #endregion

        #region ViewTimer
        public static void startViewTimer()
        {
            viewAllTimer.Enabled = true;
            viewAllTimer.Elapsed += new ElapsedEventHandler(viewTimer);
        }

        public static void stopViewTimer()
        {
            viewAllTimer.Enabled = false;
            viewAllTimer.Elapsed -= new ElapsedEventHandler(viewTimer);
        }

        public static void viewTimer(object sender, ElapsedEventArgs args)
        {
            int count = 0;
            foreach (Mplayer player in MAC.Players)
            {
                if (player.viewAll)
                {
                    foreach (TSPlayer tply in TShock.Players)
                    {
                        try
                        {
                            int prevTeam = Main.player[tply.Index].team;
                            Main.player[tply.Index].team = MAC.viewAllTeam;
                            NetMessage.SendData((int)PacketTypes.PlayerTeam, player.Index, -1, "", tply.Index);
                            Main.player[tply.Index].team = prevTeam;
                        }
                        catch (Exception) { }
                    }
                    count++;
                }

                if (count == 0)
                    stopViewTimer();

                count = 0;
            }
        }
        #endregion

        #region TimeTimer
        public static void startTimeTimer()
        {
            timeTimer.Enabled = true;
            timeTimer.Elapsed += new ElapsedEventHandler(tTimer);
        }

        public static void stopTimeTimer()
        {
            timeTimer.Enabled = false;
            timeTimer.Elapsed -= new ElapsedEventHandler(tTimer);
        }

        public static void tTimer(object sender, ElapsedEventArgs args)
        {
            if (MAC.timeFrozen)
            {
                if (Main.dayTime != MAC.freezeDayTime)
                {
                    if (MAC.timeToFreezeAt > 10000)
                    {
                        MAC.timeToFreezeAt -= 100;
                    }
                    else
                    {
                        MAC.timeToFreezeAt += 100;
                    }
                }
                TSPlayer.Server.SetTime(MAC.freezeDayTime, MAC.timeToFreezeAt);
            }
        }
        #endregion

        #region AutoKillTimer
        public static void startAutoKillTimer()
        {
            autoKillTimer.Enabled = true;
            autoKillTimer.Elapsed += new ElapsedEventHandler(aKTimer);
        }

        public static void stopAutoKillTimer()
        {
            autoKillTimer.Enabled = false;
            autoKillTimer.Elapsed -= new ElapsedEventHandler(aKTimer);
        }

        public static void aKTimer(object sender, ElapsedEventArgs args)
        {
            int count = 0;
            foreach (Mplayer player in MAC.Players)
            {
                if (!player.TSPlayer.Dead)
                {
                    if (player.autoKill)
                    {
                        player.TSPlayer.DamagePlayer(9999);
                    }
                }
                count++;
            }
            if (count == 0)
                stopAutoKillTimer();

            count = 0;
        }
        #endregion
    }
}
