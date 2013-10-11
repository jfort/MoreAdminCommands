using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using MySql.Data.MySqlClient;
using Terraria;
using TerrariaApi;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.Threading;

namespace MoreAdminCommands
{
    public class Cmds
    {

        #region FindCommand
        public static void FindCommand(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                List<string> commandNameList = new List<string>();

                foreach (Command command in Commands.ChatCommands)
                {
                    for (int i = 0; i < command.Permissions.Count; i++)
                    {
                        if (args.Player.Group.HasPermission(command.Permissions[i]))
                        {
                            foreach (string commandName in command.Names)
                            {
                                bool showCommand = true;
                                foreach (string searchParameter in args.Parameters)
                                {
                                    if (!commandName.Contains(searchParameter))
                                    {
                                        showCommand = false;
                                        break;
                                    }
                                }
                                if (showCommand && !commandNameList.Contains(commandName))
                                {
                                    commandNameList.Add(command.Name);
                                }
                            }
                        }
                    }
                }
                if (commandNameList.Count > 0)
                {
                    args.Player.SendMessage("The following commands matched your search:", Color.Yellow);
                    for (int i = 0; i < commandNameList.Count && i < 6; i++)
                    {
                        string returnLine = "";
                        for (int j = 0; j < commandNameList.Count - i * 5 && j < 5; j++)
                        {
                            if (i * 5 + j + 1 < commandNameList.Count)
                            {
                                returnLine += commandNameList[i * 5 + j] + ", ";
                            }
                            else
                            {
                                returnLine += commandNameList[i * 5 + j] + ".";
                            }
                        }
                        args.Player.SendInfoMessage(returnLine);
                    }
                }
                else
                {
                    args.Player.SendErrorMessage("No Commands matched your search term(s).");
                }
            }
            else
            {
                args.Player.SendErrorMessage("Invalid syntax. Try /findcommand [term 1] (optional[term 2] [term 3] etc)");
            }
        }
        #endregion

        #region FindPermission
        public static void FindPerms(CommandArgs args)
        {
            if (args.Parameters.Count == 1)
            {
                foreach (Command cmd in TShockAPI.Commands.ChatCommands)
                {
                    if (cmd.Names.Contains(args.Parameters[0]))
                    {
                        args.Player.SendInfoMessage(string.Format("Permission to use {0}: {1}",
                            cmd.Name, cmd.Permissions[0] != "" ? cmd.Permissions[0] : "Nothing"));
                        return;
                    }
                }
                args.Player.SendErrorMessage("Command not be found.");
            }
            else
            {
                args.Player.SendErrorMessage("Invalid syntax. Try /findperm [command]");
            }
        }
        #endregion

        #region MoonPhase
        public static void MoonPhase(CommandArgs args)
        {
            try
            {
                Main.moonPhase = Convert.ToInt32(args.Parameters[0]);
                NetMessage.SendData((int)PacketTypes.TimeSet, -1, -1, "", 0, 0, Main.sunModY, Main.moonModY);
                args.Player.SendInfoMessage("The moon phase has been changed!");
            }
            catch (Exception) { args.Player.SendErrorMessage("Invalid phase number!"); }
        }
        #endregion

        #region AutoKill
        public static void AutoKill(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                var plyList = TShockAPI.TShock.Utils.FindPlayer(args.Parameters[0]);
                if (plyList.Count > 1)
                {
                    args.Player.SendErrorMessage("Player does not exist.");
                }
                else if (plyList.Count < 1)
                {
                    args.Player.SendErrorMessage(plyList.Count.ToString() + " players matched.");
                }
                else
                {
                    if (!plyList[0].Group.HasPermission("autokill") || args.Player == plyList[0])
                    {
                        MAC.autoKill[plyList[0].Index] = !MAC.autoKill[plyList[0].Index];
                        if (MAC.autoKill[plyList[0].Index])
                        {
                            args.Player.SendInfoMessage(plyList[0].Name + " is now being auto-killed.");
                            plyList[0].SendInfoMessage("You are now being auto-killed. " + 
                                " Beg for mercy, that you may be spared.");
                        }
                        else
                        {
                            args.Player.SendInfoMessage(plyList[0].Name + " is no longer being auto-killed.");
                            plyList[0].SendInfoMessage("You have been pardoned.");
                        }
                    }
                    else
                    {
                        args.Player.SendErrorMessage("You cannot autokill someone with the autokill permission.");
                    }
                }
            }
            else
            {
                args.Player.SendErrorMessage("Invalid syntax.  Proper Syntax: /autokill playername");
            }
        }
        #endregion

        #region TeamUnlock
        public static void TeamUnlock(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                string str = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));

                var player = Utils.GetPlayers(args.Player.Index);

                switch (args.Parameters[0].ToLower())
                {
                    case "red": if (str == MAC.config.redPass)
                        {
                            player.accessRed = true;
                            args.Player.SendErrorMessage("Red team unlocked.");
                        }
                        else
                        {
                            args.Player.SendErrorMessage("Incorrect password.");
                        }
                        break;

                    case "blue": if (str == MAC.config.bluePass)
                        {
                            player.accessBlue = true;
                            args.Player.SendMessage("Blue team unlocked.", Color.LightBlue);
                        }
                        else
                        {
                            args.Player.SendErrorMessage("Incorrect password.");
                        }
                        break;

                    case "green": if (str == MAC.config.greenPass)
                        {
                            player.accessGreen = true;
                            args.Player.SendSuccessMessage("Green team unlocked.");
                        }
                        else
                        {
                            args.Player.SendErrorMessage("Incorrect password.");
                        }
                        break;

                    case "yellow": 
                        if (str == MAC.config.yellowPass)
                        {
                            player.accessYellow = true;
                            args.Player.SendInfoMessage("Yellow unlocked.");
                        }
                        else
                        {
                            args.Player.SendErrorMessage("Incorrect password.");
                        }
                        break;

                    default:
                        args.Player.SendErrorMessage("Invalid team color.");
                        break;
                }
            }
            else
            {
                args.Player.SendMessage("Improper Syntax.  Proper Syntax: /teamunlock teamcolor password", Color.Red);
            }
        }
        #endregion

        #region ViewAll
        public static void ViewAll(CommandArgs args)
        {
            var player = Utils.GetPlayers(args.Player.Index);

            player.viewAll = !player.viewAll;

            if (player.viewAll)
                args.Player.SendInfoMessage("View All mode has been turned on.");

            else
            {
                args.Player.SetTeam(Main.player[args.Player.Index].team);
                foreach (TSPlayer tply in TShock.Players)
                {
                    try
                    {
                        NetMessage.SendData((int)PacketTypes.PlayerTeam, args.Player.Index, -1, "", tply.Index);
                    }
                    catch (Exception) { }
                }
                args.Player.SendInfoMessage("View All mode has been turned off.");
            }
        }
        #endregion

        #region SpawnMobPlayer
        public static void SpawnMobPlayer(CommandArgs args)
        {
            if (args.Parameters.Count < 1 || args.Parameters.Count > 3)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /spawnmob <mob name/id> [amount] [username]", Color.Red);
                return;
            }
            if (args.Parameters[0].Length == 0)
            {
                args.Player.SendMessage("Missing mob name/id", Color.Red);
                return;
            }
            int amount = 1;
            if (args.Parameters.Count == 3 && !int.TryParse(args.Parameters[1], out amount))
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /spawnmob <mob name/id> [amount] [username]", Color.Red);
                return;
            }

            amount = Math.Min(amount, Main.maxNPCs);

            var npcs = TShockAPI.TShock.Utils.GetNPCByIdOrName(args.Parameters[0]);
            var players = TShockAPI.TShock.Utils.FindPlayer(args.Parameters[2]);
            if (players.Count == 0)
            {
                args.Player.SendMessage("Invalid player!", Color.Red);
            }
            else if (players.Count > 1)
            {
                args.Player.SendMessage("More than one player matched!", Color.Red);
            }
            else if (npcs.Count == 0)
            {
                args.Player.SendMessage("Invalid mob type!", Color.Red);
            }
            else if (npcs.Count > 1)
            {
                args.Player.SendMessage(string.Format("More than one ({0}) mob matched!", npcs.Count), Color.Red);
            }
            else
            {
                var npc = npcs[0];
                if (npc.type >= 1 && npc.type < Main.maxNPCTypes)
                {
                    TSPlayer.Server.SpawnNPC(npc.type, npc.name, amount, players[0].TileX, players[0].TileY, 50, 20);
                    TSPlayer.All.SendInfoMessage(string.Format("{0} was spawned {1} time(s) nearby {2}.", npc.name, amount, players[0].Name));
                }
                else
                    args.Player.SendMessage("Invalid mob type!", Color.Red);
            }
        }
        #endregion

        #region FreezeTime
        public void FreezeTime(CommandArgs args)
        {
            MAC.timeFrozen = !MAC.timeFrozen;
            MAC.freezeDayTime = Main.dayTime;
            MAC.timeToFreezeAt = Main.time;

            if (MAC.timeFrozen)
            {
                TSPlayer.All.SendInfoMessage(args.Player.Name.ToString() + " froze time.");
            }
            else
            {
                TSPlayer.All.SendInfoMessage(args.Player.Name.ToString() + " unfroze time.");
            }
        }
        #endregion

        #region KillAll
        private static void KillAll(CommandArgs args)
        {
            foreach (TSPlayer plr in TShock.Players)
            {
                if (plr != null)
                {
                    if (plr != args.Player)
                    {
                        plr.DamagePlayer(999999);
                    }
                }
            }
            TSPlayer.All.SendInfoMessage(args.Player.Name + " killed everyone!");
            args.Player.SendSuccessMessage("You killed everyone!");
        }
        #endregion

        #region ForceGive
        private static void ForceGive(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /forcegive <item type/id> <player> [item amount]", Color.Red);
                return;
            }
            if (args.Parameters[0].Length == 0)
            {
                args.Player.SendMessage("Missing item name/id", Color.Red);
                return;
            }
            if (args.Parameters[1].Length == 0)
            {
                args.Player.SendMessage("Missing player name", Color.Red);
                return;
            }
            int itemAmount = 0;
            var items = TShockAPI.TShock.Utils.GetItemByIdOrName(args.Parameters[0]);
            args.Parameters.RemoveAt(0);
            string plStr = args.Parameters[0];
            args.Parameters.RemoveAt(0);
            if (args.Parameters.Count > 0)
                int.TryParse(args.Parameters[args.Parameters.Count - 1], out itemAmount);


            if (items.Count == 0)
            {
                args.Player.SendMessage("Invalid item type!", Color.Red);
            }
            else if (items.Count > 1)
            {
                args.Player.SendMessage(string.Format("More than one ({0}) item matched!", items.Count), Color.Red);
            }
            else
            {
                var item = items[0];
                if (item.type >= 1 && item.type < Main.maxItemTypes)
                {
                    var players = TShockAPI.TShock.Utils.FindPlayer(plStr);
                    if (players.Count == 0)
                    {
                        args.Player.SendMessage("Invalid player!", Color.Red);
                    }
                    else if (players.Count > 1)
                    {
                        args.Player.SendMessage("More than one player matched!", Color.Red);
                    }
                    else
                    {
                        var plr = players[0];
                        int stacks = 1;
                        if (itemAmount == 0)
                            itemAmount = item.maxStack;
                        if (itemAmount > item.maxStack)
                            stacks = itemAmount / item.maxStack + 1;
                        for (int i = 1; i < stacks; i++)
                            plr.GiveItem(item.type, item.name, item.width, item.height, item.maxStack);
                        if (itemAmount - (itemAmount / item.maxStack) * item.maxStack != 0)
                            plr.GiveItem(item.type, item.name, item.width, item.height, itemAmount - (itemAmount / item.maxStack) * item.maxStack);
                        args.Player.SendMessage(string.Format("Gave {0} {1} {2}(s).", plr.Name, itemAmount, item.name));
                        plr.SendMessage(string.Format("{0} gave you {1} {2}(s).", args.Player.Name, itemAmount, item.name));
                    }
                }
                else
                {
                    args.Player.SendMessage("Invalid item type!", Color.Red);
                }
            }
        }
        #endregion

        public static void permaBuff(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Improper Syntax! Proper Syntax: /permabuff buff [player]");
            }
            else if (args.Parameters.Count == 1)
            {

                int id = 0;
                bool isGroup = false;
                if (!int.TryParse(args.Parameters[0], out id))
                {
                    var found = TShockAPI.TShock.Utils.GetBuffByName(args.Parameters[0]);
                    List<int> found2 = GetGroupBuffByName(args.Parameters[0]);
                    if (args.Parameters[0].ToLower() == "off")
                    {

                        if (buffsUsed[args.Player.Index].Count() > 0)
                        {

                            buffsUsed[args.Player.Index].Clear();
                            args.Player.SendMessage("You have had all permabuffs removed.");

                        }
                        else
                        {

                            args.Player.SendMessage("You do not currently have any permabuffs applied (solely) to yourself.");

                        }
                        return;

                    }
                    if (found.Count + found2.Count == 0)
                    {
                        args.Player.SendMessage("Invalid buff name!", Color.Red);
                        return;
                    }
                    else if (found.Count + found2.Count > 1)
                    {
                        args.Player.SendMessage(string.Format("More than one ({0}) buff matched!", found.Count), Color.Red);
                        return;
                    }
                    if (found.Count == 1)
                    {

                        id = found[0];

                    }
                    else if (found2.Count == 1)
                    {

                        id = found2[0];
                        isGroup = true;

                    }
                    else
                    {

                        return;

                    }
                }
                if (!isGroup)
                {
                    if (id > 0 && id < Main.maxBuffs)
                    {
                        if (!buffsUsed[args.Player.Index].Contains(id))
                        {
                            args.Player.SetBuff(id, short.MaxValue);
                            buffsUsed[args.Player.Index].Add(id);
                            args.Player.SendMessage(string.Format("You have permabuffed yourself with {0}({1})!",
                                TShockAPI.TShock.Utils.GetBuffName(id), TShockAPI.TShock.Utils.GetBuffDescription(id)), Color.Green);
                        }
                        else
                        {
                            buffsUsed[args.Player.Index].Remove(id);
                            args.Player.SendMessage(string.Format("You have removed your {0} permabuff.",
                                TShockAPI.TShock.Utils.GetBuffName(id)), Color.Green);

                        }
                    }
                }
                else
                {

                    foreach (int id2 in buffGroups.Values.ToArray()[id])
                    {
                        args.Player.SetBuff(id2, short.MaxValue);
                        if (!buffsUsed[args.Player.Index].Contains(id2))
                            buffsUsed[args.Player.Index].Add(id2);
                        args.Player.SendMessage(string.Format("You have permabuffed yourself with {0}({1})!",
                            TShockAPI.TShock.Utils.GetBuffName(id2), TShockAPI.TShock.Utils.GetBuffDescription(id2)), Color.Green);
                    }

                }

            }
            else
            {

                string str = "";
                for (int i = 1; i < args.Parameters.Count; i++)
                {

                    if (i != args.Parameters.Count - 1)
                    {

                        str += args.Parameters[i] + " ";

                    }
                    else
                    {

                        str += args.Parameters[i];

                    }

                }
                List<TShockAPI.TSPlayer> playerList = TShockAPI.TShock.Utils.FindPlayer(str);
                if (playerList.Count > 1)
                {

                    args.Player.SendMessage("Player does not exist.", Color.Red);

                }
                else if (playerList.Count < 1)
                {

                    args.Player.SendMessage(playerList.Count.ToString() + " players matched.", Color.Red);

                }
                else
                {

                    TShockAPI.TSPlayer thePlayer = playerList[0];
                    int id = 0;
                    bool isGroup = false;
                    if (!int.TryParse(args.Parameters[0], out id))
                    {
                        var found = TShockAPI.TShock.Utils.GetBuffByName(args.Parameters[0]);
                        List<int> found2 = GetGroupBuffByName(args.Parameters[0]);
                        if (args.Parameters[0].ToLower() == "off")
                        {

                            if (buffsUsed[thePlayer.Index].Count() > 0)
                            {

                                buffsUsed[thePlayer.Index].Clear();
                                args.Player.SendMessage("You have had all permabuffs removed from " + thePlayer.Name + ".");
                                TShock.Players[thePlayer.Index].SendMessage("You have had all permabuffs removed.");

                            }
                            else
                            {

                                args.Player.SendMessage("You do not currently have any permabuffs applied (solely) to " + thePlayer.Name + ".");

                            }
                            return;

                        }
                        if (found.Count + found2.Count == 0)
                        {
                            args.Player.SendMessage("Invalid buff name!", Color.Red);
                            return;
                        }
                        else if (found.Count + found2.Count > 1)
                        {
                            args.Player.SendMessage(string.Format("More than one ({0}) buff matched!", found.Count), Color.Red);
                            return;
                        }
                        if (found.Count == 1)
                        {

                            id = found[0];

                        }
                        else if (found2.Count == 1)
                        {

                            id = found2[0];
                            isGroup = true;

                        }
                        else
                        {

                            return;

                        }
                    }
                    if (!isGroup)
                    {
                        if (id > 0 && id < Main.maxBuffs)
                        {
                            if (!buffsUsed[thePlayer.Index].Contains(id))
                            {
                                thePlayer.SetBuff(id, short.MaxValue);
                                buffsUsed[thePlayer.Index].Add(id);
                                args.Player.SendMessage(string.Format("You have permabuffed " + thePlayer.Name + " with {0}",
                                    TShockAPI.TShock.Utils.GetBuffName(id)), Color.Green);
                                thePlayer.SendMessage(string.Format("You have been permabuffed with {0}({1})!",
                                 TShockAPI.TShock.Utils.GetBuffName(id), TShockAPI.TShock.Utils.GetBuffDescription(id)), Color.Green);
                            }
                            else
                            {
                                buffsUsed[thePlayer.Index].Remove(id);
                                args.Player.SendMessage(string.Format("You have removed " + thePlayer.Name + "'s {0} permabuff.",
                                    TShockAPI.TShock.Utils.GetBuffName(id)), Color.Green);
                                thePlayer.SendMessage(string.Format("Your {0} permabuff has been removed.",
                                    TShockAPI.TShock.Utils.GetBuffName(id)), Color.Green);

                            }
                        }
                    }
                    else
                    {
                        foreach (int id2 in buffGroups.Values.ToArray()[id])
                        {
                            thePlayer.SetBuff(id2, short.MaxValue);
                            if (!buffsUsed[thePlayer.Index].Contains(id2))
                                buffsUsed[thePlayer.Index].Add(id2);
                            args.Player.SendMessage(string.Format("You have permabuffed " + thePlayer.Name + " with {0}",
                                TShockAPI.TShock.Utils.GetBuffName(id2)), Color.Green);
                            thePlayer.SendMessage(string.Format("You have been permabuffed with {0}({1})!",
                             TShockAPI.TShock.Utils.GetBuffName(id2), TShockAPI.TShock.Utils.GetBuffDescription(id)), Color.Green);
                        }
                    }

                }

            }

        }

        public static void permaBuffAll(CommandArgs args)
        {

            if (args.Parameters.Count == 0)
            {

                args.Player.SendMessage("Improper Syntax! Proper Syntax: /permabuffall buff [player]", Color.Red);

            }
            else if (args.Parameters.Count == 1)
            {

                int id = 0;
                bool isGroup = false;
                if (!int.TryParse(args.Parameters[0], out id))
                {
                    var found = TShockAPI.TShock.Utils.GetBuffByName(args.Parameters[0]);
                    List<int> found2 = GetGroupBuffByName(args.Parameters[0]);
                    if (args.Parameters[0].ToLower() == "off")
                    {

                        if (allBuffsUsed.Count() > 0)
                        {

                            allBuffsUsed.Clear();
                            TSPlayer.All.SendInfoMessage("All Global permabuffs have been deactivated.");

                        }
                        else
                        {

                            args.Player.SendMessage("There are currently no global permabuffs active.");

                        }
                        return;

                    }
                    if (found.Count + found2.Count == 0)
                    {
                        args.Player.SendMessage("Invalid buff name!", Color.Red);
                        return;
                    }
                    else if (found.Count + found2.Count > 1)
                    {
                        args.Player.SendMessage(string.Format("More than one ({0}) buff matched!", found.Count), Color.Red);
                        return;
                    }
                    if (found.Count == 1)
                    {

                        id = found[0];

                    }
                    else if (found2.Count == 1)
                    {

                        id = found2[0];
                        isGroup = true;

                    }
                    else
                    {

                        return;

                    }
                }
                if (!isGroup)
                {
                    if (id > 0 && id < Main.maxBuffs)
                    {
                        if (!allBuffsUsed.Contains(id))
                        {
                            TSPlayer.All.SetBuff(id, short.MaxValue);
                            allBuffsUsed.Add(id);
                            TSPlayer.All.SendInfoMessage(string.Format("Everyone has been permabuffed with {0}({1})!",
                                TShockAPI.TShock.Utils.GetBuffName(id), TShockAPI.TShock.Utils.GetBuffDescription(id)), Color.Green);
                        }
                        else
                        {
                            allBuffsUsed.Remove(id);
                            TSPlayer.All.SendInfoMessage(string.Format("Everyone has had the {0} permabuff removed.",
                                TShockAPI.TShock.Utils.GetBuffName(id)), Color.Green);

                        }
                    }
                }
                else
                {

                    foreach (int id2 in buffGroups.Values.ToArray()[id])
                    {
                        TSPlayer.All.SetBuff(id2, short.MaxValue);
                        if (!allBuffsUsed.Contains(id2))
                            allBuffsUsed.Add(id2);
                        TSPlayer.All.SendInfoMessage(string.Format("Everyone has been permabuffed with {0}({1})!",
                            TShockAPI.TShock.Utils.GetBuffName(id2), TShockAPI.TShock.Utils.GetBuffDescription(id2)), Color.Green);
                    }

                }

            }

        }

        public static void Mow(CommandArgs args)
        {

            if (args.Parameters.Count > 0)
            {

                string str = "";
                for (int i = 0; i < args.Parameters.Count; i++)
                {

                    if (i != args.Parameters.Count - 1)
                    {

                        str += args.Parameters[i] + " ";

                    }
                    else
                    {

                        str += args.Parameters[i];

                    }

                }
                TShockAPI.DB.Region theRegion = TShock.Regions.GetRegionByName(str);
                if (theRegion != default(TShockAPI.DB.Region))
                {
                    try
                    {
                        int index = SearchTable(SQLEditor.ReadColumn("regionMow", "Name", new List<SqlValue>()), str);
                        if (index == -1)
                        {

                            List<SqlValue> theList = new List<SqlValue>();
                            theList.Add(new SqlValue("Name", "'" + str + "'"));
                            theList.Add(new SqlValue("Mow", 1));
                            SQLEditor.InsertValues("regionMow", theList);
                            regionMow.Add(str, true);
                            args.Player.SendMessage(str + " is now set to auto-mow.");

                        }
                        else if (Convert.ToBoolean(SQLEditor.ReadColumn("regionMow", "Mow", new List<SqlValue>())[index]))
                        {

                            List<SqlValue> theList = new List<SqlValue>();
                            List<SqlValue> where = new List<SqlValue>();
                            theList.Add(new SqlValue("Mow", 0));
                            where.Add(new SqlValue("Name", "'" + str + "'"));
                            SQLEditor.UpdateValues("regionMow", theList, where);
                            regionMow.Remove(str);
                            regionMow.Add(str, false);
                            args.Player.SendMessage(str + " now has auto-mow turned off.");

                        }
                        else
                        {

                            List<SqlValue> theList = new List<SqlValue>();
                            List<SqlValue> where = new List<SqlValue>();
                            theList.Add(new SqlValue("Mow", 1));
                            where.Add(new SqlValue("Name", "'" + str + "'"));
                            SQLEditor.UpdateValues("regionMow", theList, where);
                            regionMow.Remove(str);
                            regionMow.Add(str, true);
                            args.Player.SendMessage(str + " is now set to auto-mow.");

                        }
                    }
                    catch (Exception) { args.Player.SendMessage("An error occurred when writing to the DataBase.", Color.Red); }

                }
                else
                {

                    args.Player.SendMessage("The specified region does not exist.");

                }

            }
            else
            {

                args.Player.SendMessage("Improper Syntax.  Proper Syntax: /mow regionname", Color.Red);

            }

        }

        public static void AutoHeal(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                isHeal[args.Player.Index] = !isHeal[args.Player.Index];
                if (isHeal[args.Player.Index])
                {

                    args.Player.SendMessage("Auto Heal Mode is now on.");

                }
                else
                {

                    args.Player.SendMessage("Auto Heal Mode is now off.");

                }
            }
            else
            {

                string str = "";
                for (int i = 0; i < args.Parameters.Count; i++)
                {

                    if (i != args.Parameters.Count - 1)
                    {

                        str += args.Parameters[i] + " ";

                    }
                    else
                    {

                        str += args.Parameters[i];

                    }

                }
                List<TShockAPI.TSPlayer> playerList = TShockAPI.TShock.Utils.FindPlayer(str);
                if (playerList.Count > 1)
                {

                    args.Player.SendMessage("Player does not exist.", Color.Red);

                }
                else if (playerList.Count < 1)
                {

                    args.Player.SendMessage(playerList.Count.ToString() + " players matched.", Color.Red);

                }
                else
                {

                    TShockAPI.TSPlayer thePlayer = playerList[0];
                    isHeal[thePlayer.Index] = !isHeal[thePlayer.Index];
                    if (isHeal[thePlayer.Index])
                    {

                        args.Player.SendMessage("You have activated auto-heal for " + thePlayer.Name + ".");
                        thePlayer.SendMessage("You have been given regenerative powers!");

                    }
                    else
                    {

                        args.Player.SendMessage("You have deactivated auto-heal for " + thePlayer.Name + ".");
                        thePlayer.SendMessage("You now have the healing powers of an average human.");

                    }

                }

            }

        }

        public static void Ghost(CommandArgs args)
        {

            if (args.Parameters.Count == 0)
            {
                int tempTeam = args.Player.TPlayer.team;
                args.Player.TPlayer.team = 0;
                NetMessage.SendData(45, -1, -1, "", args.Player.Index);
                args.Player.TPlayer.team = tempTeam;
                if (!isGhost[args.Player.Index])
                {

                    args.Player.SendMessage("Ghost Mode activated!");
                    TSPlayer.All.SendInfoMessage(args.Player.Name + " left", Color.Yellow);

                }
                else
                {

                    args.Player.SendMessage("Ghost Mode deactivated!");
                    TSPlayer.All.SendInfoMessage(args.Player.Name + " has joined.", Color.Yellow);

                }
                isGhost[args.Player.Index] = !isGhost[args.Player.Index];
                args.Player.TPlayer.position.X = 0;
                args.Player.TPlayer.position.Y = 0;
                cansend = true;
                NetMessage.SendData(13, -1, -1, "", args.Player.Index);
                cansend = false;
            }
            else
            {

                string str = "";
                for (int i = 0; i < args.Parameters.Count; i++)
                {

                    if (i != args.Parameters.Count - 1)
                    {

                        str += args.Parameters[i] + " ";

                    }
                    else
                    {

                        str += args.Parameters[i];

                    }

                }
                List<TShockAPI.TSPlayer> playerList = TShockAPI.TShock.Utils.FindPlayer(str);
                if (playerList.Count > 1)
                {

                    args.Player.SendMessage("Player does not exist.", Color.Red);

                }
                else if (playerList.Count < 1)
                {

                    args.Player.SendMessage(playerList.Count.ToString() + " players matched.", Color.Red);

                }
                else
                {

                    TShockAPI.TSPlayer thePlayer = playerList[0];
                    int tempTeam = thePlayer.TPlayer.team;
                    thePlayer.TPlayer.team = 0;
                    NetMessage.SendData(45, -1, -1, "", thePlayer.Index);
                    thePlayer.TPlayer.team = tempTeam;
                    if (!isGhost[thePlayer.Index])
                    {

                        args.Player.SendMessage("Ghost Mode activated for " + thePlayer.Name + ".");
                        thePlayer.SendMessage("You have become a stealthy ninja!");

                    }
                    else
                    {

                        args.Player.SendMessage("Ghost Mode deactivated for " + thePlayer.Name + ".");
                        thePlayer.SendMessage("You no longer have the stealth of a ninja.");

                    }
                    isGhost[thePlayer.Index] = !isGhost[thePlayer.Index];
                    thePlayer.TPlayer.position.X = 0;
                    thePlayer.TPlayer.position.Y = 0;
                    cansend = true;
                    NetMessage.SendData(13, -1, -1, "", thePlayer.Index);
                    cansend = false;

                }

            }

        }

        public static void SpawnGroup(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                try
                {
                    Dictionary<NPC, int> groupSpawn = MAC.GetSpawnBuffByName(args.Parameters[0]);
                    if (groupSpawn.Count < 1)
                    {
                        args.Player.SendMessage("Invalid Spawn Group name.", Color.Red);
                    }
                    else
                    {
                        if (args.Parameters.Count > 1)
                        {
                            try
                            {
                                double multiplier = Convert.ToDouble(args.Parameters[1]);
                                foreach (KeyValuePair<NPC, int> entry in groupSpawn)
                                {
                                    int amount = (int)(entry.Value * multiplier);
                                    if (amount > 1000)
                                    {
                                        amount = 1000;
                                    }
                                    TSPlayer.Server.SpawnNPC(entry.Key.type, entry.Key.name, amount, args.Player.TileX, args.Player.TileY, 50, 20);
                                    TSPlayer.All.SendInfoMessage(entry.Key.name + " was spawned " + amount.ToString() + " times.");

                                }

                            }
                            catch (Exception) { args.Player.SendMessage("Invalid Syntax.  Proper Syntax: /spawngroup spawngroupname [multiplier]", Color.Red); }

                        }
                        else
                        {

                            foreach (KeyValuePair<NPC, int> entry in groupSpawn)
                            {

                                TSPlayer.Server.SpawnNPC(entry.Key.type, entry.Key.name, entry.Value, args.Player.TileX, args.Player.TileY, 50, 20);
                                TSPlayer.All.SendInfoMessage(entry.Key.name + " was spawned " + entry.Value.ToString() + " times.");

                            }

                        }

                    }

                }
                catch (Exception) { args.Player.SendMessage("Invalid spawn group name.", Color.Red); }

            }
            else
            {

                args.Player.SendMessage("Invalid Syntax.  Proper Syntax: /spawngroup spawngroupname [multiplier]");

            }

        }

        public static void SpawnAll(CommandArgs args)
        {

            int amount = 1;
            if (args.Parameters.Count > 0)
            {

                try
                {

                    amount = Convert.ToInt32(args.Parameters[0]);

                }
                catch (Exception) { args.Player.SendMessage("Improper Syntax.  Proper Syntax: /spawnall [amount]", Color.Red); return; }

            }
            for (int i = 0; i < Main.maxNPCTypes; i++)
            {

                var npc = TShockAPI.TShock.Utils.GetNPCById(i);
                if (!npc.name.ToLower().StartsWith("dungeon guar"))
                    TSPlayer.Server.SpawnNPC(npc.type, npc.name, amount, args.Player.TileX, args.Player.TileY, 50, 20);

            }
            if (amount > 1000 / Main.maxNPCTypes)
            {

                amount = 1000 / Main.maxNPCTypes;

            }
            TSPlayer.All.SendInfoMessage(args.Player.Name + " has spawned every npc " + amount.ToString() + " times!");

        }

        public static void ReloadMore(CommandArgs args)
        {

            reload();
            args.Player.SendMessage("More Admin Commands Config file successfully reloaded.");

        }

        public static void permaBuffGroup(CommandArgs args)
        {

            if (args.Parameters.Count() > 1)
            {

                string str = args.Parameters[0].ToLower();
                if (TShock.Groups.GroupExists(str))
                {

                    List<int> buffs = TShockAPI.TShock.Utils.GetBuffByName(args.Parameters[1]);
                    List<int> buffs2 = GetGroupBuffByName(args.Parameters[1]);
                    bool isGroup = false;
                    if (args.Parameters[1].ToLower() == "off")
                    {

                        if (!buffsUsedGroup.ContainsKey(str))
                        {

                            args.Player.SendMessage("There are no permabuffs currently applied to this group.", Color.Red);

                        }
                        else
                        {
                            try
                            {

                                buffsUsedGroup[str].Clear();

                            }
                            catch (Exception) { }
                            args.Player.SendMessage("The " + str + " group has had all buffs removed.");
                        }
                        return;

                    }
                    if (buffs.Count + buffs2.Count < 1)
                    {

                        args.Player.SendMessage("No buffs by that name can be found.", Color.Red);

                    }
                    else if (buffs.Count + buffs2.Count > 1)
                    {

                        args.Player.SendMessage("More than one buff matched.", Color.Red);

                    }
                    else
                    {

                        if (buffs2.Count == 1)
                        {

                            isGroup = true;

                        }
                        if (!buffsUsedGroup.ContainsKey(str))
                        {

                            if (!isGroup)
                            {
                                List<int> tempList = new List<int>();
                                tempList.Add(buffs[0]);
                                buffsUsedGroup.Add(str, tempList);
                                args.Player.SendMessage("The " + str + " group has had the " + TShockAPI.TShock.Utils.GetBuffName(buffs[0]) + " permabuff added.");
                            }
                            else
                            {

                                foreach (int id in buffGroups.Values.ToArray()[buffs2[0]])
                                {
                                    List<int> tempList = new List<int>();
                                    tempList.Add(id);
                                    List<int> tempList2 = new List<int>();
                                    if (!buffsUsedGroup.Keys.ToArray().Contains(str))
                                    {
                                        buffsUsedGroup.Add(str, tempList);
                                    }
                                    else
                                    {
                                        buffsUsedGroup.TryGetValue(str, out tempList2);
                                        buffsUsedGroup.Remove(str);
                                        tempList2.Add(id);
                                        buffsUsedGroup.Add(str, tempList2);

                                    }
                                    args.Player.SendMessage("The " + str + " group has had the " + TShockAPI.TShock.Utils.GetBuffName(id) + " permabuff added.");
                                }

                            }

                        }
                        else
                        {

                            try
                            {
                                List<int> tempChangeList;
                                buffsUsedGroup.TryGetValue(str, out tempChangeList);
                                if (!isGroup)
                                {
                                    if (!tempChangeList.Contains(buffs[0]))
                                    {

                                        tempChangeList.Add(buffs[0]);
                                        args.Player.SendMessage("The " + str + " group has had the " + TShockAPI.TShock.Utils.GetBuffName(buffs[0]) + " permabuff added.");

                                    }
                                    else
                                    {

                                        tempChangeList.Remove(buffs[0]);
                                        args.Player.SendMessage("The " + str + " group has had the " + TShockAPI.TShock.Utils.GetBuffName(buffs[0]) + " permabuff removed.");

                                    }
                                }
                                else
                                {

                                    foreach (int id in buffGroups.Values.ToArray()[buffs2[0]])
                                    {

                                        if (!tempChangeList.Contains(id))
                                        {

                                            tempChangeList.Add(id);
                                            args.Player.SendMessage("The " + str + " group has had the " + TShockAPI.TShock.Utils.GetBuffName(id) + " permabuff added.");

                                        }
                                        else
                                        {

                                            tempChangeList.Remove(id);
                                            args.Player.SendMessage("The " + str + " group has had the " + TShockAPI.TShock.Utils.GetBuffName(id) + " permabuff removed.");

                                        }

                                    }

                                }
                                buffsUsedGroup.Remove(str);
                                buffsUsedGroup.Add(str, tempChangeList);

                            }
                            catch (Exception) { args.Player.SendMessage("There was an error with the command, please report this to the plugin developer.", Color.Red); }

                        }

                    }

                }
                else
                {

                    args.Player.SendMessage("The specified group does not exist.", Color.Red);

                }

            }
            else
            {

                args.Player.SendMessage("Improper Syntax. Proper Syntax: /permabuffgroup groupname buffname", Color.Red);

            }

        }

        public static void MuteAll(CommandArgs args)
        {

            MAC.muteAll = !MAC.muteAll;
            if (MAC.muteAll)
            {
                MAC.config.muteAllReason = "";
                for (int i = 0; i < args.Parameters.Count; i++)
                {

                    MAC.config.muteAllReason += args.Parameters[i];
                    if (i < args.Parameters.Count - 1)
                    {

                        MAC.config.muteAllReason += " ";

                    }

                }
                if (MAC.config.muteAllReason == "")
                {

                    MAC.config.muteAllReason = MAC.config.defaultMuteAllReason;

                }
                TSPlayer.All.SendInfoMessage(args.Player.Name + " has muted everyone.");
                args.Player.SendMessage("You have muted everyone without the mute permission.  They will remain muted until you use /muteall again.");
            }
            else
            {
                for (int i = 0; i < 256; i++)
                {

                    MAC.muteAllFree[i] = false;

                }
                TSPlayer.All.SendInfoMessage(args.Player.Name + " has unmuted everyone, except perhaps those muted before everyone was muted.");
            }


        }

        public static void SpawnByMe(CommandArgs args)
        {

            if (args.Parameters.Count < 1 || args.Parameters.Count > 2)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /spawnbyme <mob name/id> [amount]", Color.Red);
                return;
            }
            if (args.Parameters[0].Length == 0)
            {
                args.Player.SendMessage("Missing mob name/id", Color.Red);
                return;
            }
            int amount = 1;
            if (args.Parameters.Count >= 2 && !int.TryParse(args.Parameters[1], out amount))
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /spawnbyme <mob name/id> [amount]", Color.Red);
                return;
            }

            amount = Math.Min(amount, Main.maxNPCs);

            var npcs = TShockAPI.TShock.Utils.GetNPCByIdOrName(args.Parameters[0]);
            if (npcs.Count == 0)
            {
                args.Player.SendMessage("Invalid mob type!", Color.Red);
            }
            else if (npcs.Count > 1)
            {
                args.Player.SendMessage(string.Format("More than one ({0}) mob matched!", npcs.Count), Color.Red);
            }
            else
            {
                var npc = npcs[0];
                if (npc.type >= 1 && npc.type < Main.maxNPCTypes)
                {
                    TSPlayer.Server.SpawnNPC(npc.type, npc.name, amount, args.Player.TileX, args.Player.TileY, 2, 3);
                    TSPlayer.All.SendInfoMessage(string.Format("{0} was spawned {1} time(s).", npc.name, amount));
                }
                else
                    args.Player.SendMessage("Invalid mob type!", Color.Red);
            }

        }

        public static void ButcherNear(CommandArgs args)
        {

            int nearby = 50;
            if (args.Parameters.Count > 0)
            {
                try
                {

                    nearby = Convert.ToInt32(args.Parameters[0]);

                }
                catch (Exception) { args.Player.SendMessage("Improper Syntax. Proper Syntax: /butchernear [distance]"); return; }
            }
            int killcount = 0;
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if ((Main.npc[i].active) && (MAC.distance(new Vector2(Main.item[i].position.X, Main.item[i].position.Y), new Point((int)Main.player[args.Player.Index].position.X, (int)Main.player[args.Player.Index].position.Y)) < nearby * 16))
                {

                    TSPlayer.Server.StrikeNPC(i, 99999, 90f, 1);
                    killcount++;
                }
            }
            TSPlayer.All.SendInfoMessage(string.Format("Killed {0} NPCs within a radius of " + nearby.ToString() + " blocks.", killcount));

        }

        public static void ButcherAll(CommandArgs args)
        {

            int killcount = 0;
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if (Main.npc[i].active)
                {
                    TSPlayer.Server.StrikeNPC(i, 99999, 90f, 1);
                    killcount++;
                }
            }
            TSPlayer.All.SendInfoMessage(string.Format("Killed {0} NPCs.", killcount));

        }

        public static void ButcherFriendly(CommandArgs args)
        {

            int killcount = 0;
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if (Main.npc[i].active && Main.npc[i].townNPC)
                {
                    TSPlayer.Server.StrikeNPC(i, 99999, 90f, 1);
                    killcount++;
                }
            }
            TSPlayer.All.SendInfoMessage(string.Format("Killed {0} friendly NPCs.", killcount));

        }

        public static void ButcherNPC(CommandArgs args)
        {

            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /butchernpc <npc name/id>", Color.Red);
                return;
            }
            if (args.Parameters[0].Length == 0)
            {
                args.Player.SendMessage("Missing npc name/id", Color.Red);
                return;
            }
            var npcs = TShockAPI.TShock.Utils.GetNPCByIdOrName(args.Parameters[0]);
            if (npcs.Count == 0)
            {
                args.Player.SendMessage("Invalid npc type!", Color.Red);
            }
            else if (npcs.Count > 1)
            {
                args.Player.SendMessage(string.Format("More than one ({0}) npc matched!", npcs.Count), Color.Red);
            }
            else
            {
                var npc = npcs[0];
                if (npc.type >= 1 && npc.type < Main.maxNPCTypes)
                {
                    int killcount = 0;
                    for (int i = 0; i < Main.npc.Length; i++)
                    {
                        if (Main.npc[i].active && Main.npc[i].type == npc.type)
                        {
                            TSPlayer.Server.StrikeNPC(i, 99999, 90f, 1);
                            killcount++;
                        }
                    }
                    TSPlayer.All.SendInfoMessage(string.Format("Killed {0} " + npc.name + "(s).", killcount));
                }
                else
                    args.Player.SendMessage("Invalid npc type!", Color.Red);
            }

        }

        public static void PermaMute(CommandArgs args)
        {

            if (args.Parameters.Count() > 0)
            {

                List<TSPlayer> tply = TShockAPI.TShock.Utils.FindPlayer(args.Parameters[0]);
                var readTableName = MAC.SQLEditor.ReadColumn("muteList", "Name", new List<SqlValue>());
                var readTableIP = MAC.SQLEditor.ReadColumn("muteList", "IP", new List<SqlValue>());
                if (tply.Count() > 1)
                {

                    args.Player.SendMessage("More than 1 player matched.", Color.Red);

                }
                else if (tply.Count() < 1)
                {

                    if (readTableName.Contains(args.Parameters[0].ToLower()))
                    {

                        List<SqlValue> theList = new List<SqlValue>();
                        List<SqlValue> where = new List<SqlValue>();
                        where.Add(new SqlValue("Name", "'" + args.Parameters[0].ToLower() + "'"));
                        MAC.SQLWriter.DeleteRow("muteList", where);
                        args.Player.SendMessage(args.Parameters[0] + " has been successfully been removed from the perma-mute list.");

                    }
                    else
                    {

                        args.Player.SendMessage("No players found under that name on the server or in the perma-mute list.", Color.Red);

                    }

                }
                else
                {

                    MAC.muteTime[tply[0].Index] = -1;
                    string str = tply[0].Name.ToLower();
                    int index = MAC.SearchTable(MAC.SQLEditor.ReadColumn("muteList", "Name", new List<SqlValue>()), str);
                    if (index == -1)
                    {

                        List<SqlValue> theList = new List<SqlValue>();
                        theList.Add(new SqlValue("Name", "'" + str + "'"));
                        theList.Add(new SqlValue("IP", "'" + tply[0].IP + "'"));
                        MAC.SQLEditor.InsertValues("muteList", theList);
                        MAC.muted[tply[0].Index] = true;
                        args.Player.SendMessage(tply[0].Name + " has been permamuted by his/her IP Address.");
                        tply[0].SendMessage("You have been muted by an admin.", Color.Red);

                    }
                    else
                    {

                        List<SqlValue> where = new List<SqlValue>();
                        where.Add(new SqlValue("IP", "'" + tply[0].IP + "'"));
                        MAC.SQLWriter.DeleteRow("muteList", where);
                        MAC.muted[tply[0].Index] = false;
                        args.Player.SendMessage(tply[0].Name + " has been taken off the perma-mute list, and is now un-muted.");
                        tply[0].SendMessage("You have been unmuted.");

                    }

                }

            }
            else
            {

                args.Player.SendMessage("Improper Syntax.  Proper Syntax: /permamute player", Color.Red);

            }

        }
    }
}
