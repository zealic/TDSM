using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Terraria_Server;
using Terraria_Server.Misc;
using Terraria_Server.Logging;
using Terraria_Server.Commands;
using Terraria_Server.Definitions;
using Terraria_Server.Permissions;

using Regions.RegionWork;
using Terraria_Server.Plugins;
using MySql.Data.MySqlClient;
using System.Data;

namespace Regions
{
    public class Regions : BasePlugin
    {
        public static int SelectorItem = 0;
        public static bool UsingPermissions = false;

        public Regions()
        {
            base.Name = "Regions";
            base.Description = "A region plugin for TDSM";
            base.Author = "DeathCradle";
            base.Version = "8";
            base.TDSMBuild = 38;
        }

        public static string RegionsFolder
        {
            get
            {
                return Statics.PluginPath + Path.DirectorySeparatorChar + "Regions";
            }
        }

        public static string DataFolder
        {
            get
            {
                return RegionsFolder + Path.DirectorySeparatorChar + "Data";
            }
        }

        public Properties rProperties { get; set; }
        #region mysql properties

        PropertiesFile mysql;

        bool mysqlenabled
        {
            get { return mysql.getValue("mysql-enabled", false); }
        }

        string mysqlserver
        {
            get { return mysql.getValue("mysql-server", "localhost"); }
        }

        string mysqldatabase
        {
            get { return mysql.getValue("mysql-database", "terraria"); }
        }

        string mysqluser
        {
            get { return mysql.getValue("mysql-user", "terraria"); }
        }

        string mysqlpassword
        {
            get { return mysql.getValue("mysql-userpass", ""); }
        }

        bool imported
        {
            get { return mysql.getValue("regionfiles-imported", false); }
        }

        string connectionString
        {
            get { return "Server=" + mysqlserver + ";" + "Database=" + mysqldatabase + ";" + "User ID=" + mysqluser + ";" + "Password=" + mysqlpassword + ";" + "Pooling=false"; }
        }

        #endregion
        public RegionManager regionManager { get; set; }
        private bool SelectorPos = true; //false for 1st (mousePoints[0]), true for 2nd
        public Selection selection;

        public Commands commands;

        public Node ChestBreak;
        public Node ChestOpen;
        public Node DoorChange;
        public Node LiquidFlow;
        public Node ProjectileUse;
        public Node SignEdit;
        public Node TileBreak;
        public Node TilePlace;

        public HookResult WorldAlter = HookResult.IGNORE;

        protected override void Initialized(object state)
        {
            if (!Directory.Exists(RegionsFolder))
                Directory.CreateDirectory(RegionsFolder);

            rProperties = new Properties(RegionsFolder + Path.DirectorySeparatorChar + "regions.properties");
            rProperties.Load();

            rProperties.AddHeaderLine("Use 'rectify=false' to ignore world alterations from");
            rProperties.AddHeaderLine("players who are blocked; Possibly saving bandwidth.");

            rProperties.pushData();
            rProperties.Save(false);

            if (rProperties.RectifyChanges)
                WorldAlter = HookResult.RECTIFY;
            
            SelectorItem = rProperties.SelectionToolID;
            #region set up mysql properties

            string pluginFolder = Statics.PluginPath + Path.DirectorySeparatorChar + "mysql";
            if (!Directory.Exists(pluginFolder))
            {
                Directory.CreateDirectory(pluginFolder);
            }

            mysql = new PropertiesFile(pluginFolder + Path.DirectorySeparatorChar + "mysql.properties");
            mysql.Load();
            var dummy1 = mysqlenabled;
            var dummy2 = mysqlserver;
            var dummy3 = mysqldatabase;
            var dummy4 = mysqluser;
            var dummy5 = mysqlpassword;
            var dummy6 = imported;
            mysql.Save(false);

            #endregion

            #region check if mysql table exists
            if (mysqlenabled)
            {
                try
                {
                    checkTable(connectionString, "terraria_regions");
                }
                catch (MySqlException e)
                {
                    if (e.Number == 1042)
                    {
                        ProgramLog.Error.Log("[Regions] Could not connect to mysql server. Falling back to using regions files");
                        mysql.setValue("mysql-enabled", "False");
                        mysql.Save();
                    }
                    else
                    {
                        ProgramLog.Error.Log("[Regions] MYSQL ERROR CODE: " + e.Number);
                        ProgramLog.Error.Log(e.StackTrace);
                    }
                }
            }
            #endregion

            regionManager = new RegionManager(DataFolder, mysqlenabled, connectionString);
            selection = new Selection();

            commands = new Commands();
            commands.regionManager = regionManager;
            commands.RegionsPlugin = this;
            commands.selection = selection;

            commands.Node_Create        = Node.FromPath("region.create");
            commands.Node_Here          = Node.FromPath("region.here");
            commands.Node_List          = Node.FromPath("region.list");
            commands.Node_Npcres        = Node.FromPath("region.npcres");
            commands.Node_Opres         = Node.FromPath("region.opres");
            commands.Node_Projectile    = Node.FromPath("region.projectile");
            commands.Node_ProtectAll    = Node.FromPath("region.protectall");
            commands.Node_Select        = Node.FromPath("region.select");
            commands.Node_User          = Node.FromPath("region.user");

            AddCommand("region")
                .WithAccessLevel(AccessLevel.OP)
                .WithHelpText("Usage:    region [select, create, user, list, npcres, opres]")
                .WithDescription("Region Management.")
                .WithPermissionNode("regions")
                .Calls(commands.Region);

            AddCommand("regions")
                .WithAccessLevel(AccessLevel.OP)
                .WithHelpText("Usage:    regions [select, create, user, list, npcres, opres]")
                .WithDescription("Region Management.")
                .WithPermissionNode("regions") //Need another method to split the commands up.
                .Calls(commands.Region);

            ChestBreak      = AddAndCreateNode("region.chestbreak");
            ChestOpen       = AddAndCreateNode("region.chestopen");
            DoorChange      = AddAndCreateNode("region.doorchange");
            LiquidFlow      = AddAndCreateNode("region.liquidflow");
            ProjectileUse   = AddAndCreateNode("region.projectileuse");
            SignEdit        = AddAndCreateNode("region.signedit");
            TileBreak       = AddAndCreateNode("region.tilebreak");
            TilePlace       = AddAndCreateNode("region.tileplace");
        }

        protected override void Enabled()
        {
            ProgramLog.Plugin.Log("Regions for TDSM #{0} enabled.", base.TDSMBuild);
        }

        protected override void Disabled()
        {
            ProgramLog.Plugin.Log("Regions disabled.");
        }

        public void Log(string fmt, params object[] args)
        {
            foreach (string line in String.Format(fmt, args).Split('\n'))
            {
                ProgramLog.Plugin.Log("[Regions] " + line);
            }
        }
        
        #region Events

            [Hook(HookOrder.NORMAL)]
            void OnPluginsLoaded(ref HookContext ctx, ref HookArgs.PluginsLoaded args)
            {
                UsingPermissions = IsRunningPermissions();
                if (UsingPermissions)
                    Log("Using Permissions.");
                else
                    Log("No Permissions Found\nUsing Internal User System");
            }

            [Hook(HookOrder.NORMAL)]
            void OnServerStateChange(ref HookContext ctx, ref HookArgs.ServerStateChange args)
            {
                if (args.ServerChangeState == ServerState.LOADED)
                {
                    //imports all the region files into the mysql table 
                    //on the first time the plugin is loaded with mysql enabled
                    if (!imported && mysqlenabled)
                    {
                        int numimported = regionManager.import();
                        mysql.setValue("regionfiles-imported", "True");
                        mysql.Save(false);
                        ProgramLog.Plugin.Log("Imported {0} Regions.", numimported);
                    }

                    regionManager.LoadRegions();
                }
            }
                    
            [Hook(HookOrder.NORMAL)]
            void OnPlayerEnteredGame(ref HookContext ctx, ref HookArgs.PlayerEnteredGame args)
            {
                /* If a player left without finishing the region, Clear it or the next player can use it. */
                if (selection.isInSelectionlist(ctx.Player))
                    selection.RemovePlayer(ctx.Player);
            }

            [Hook(HookOrder.NORMAL)]
            void OnPlayerWorldAlteration(ref HookContext ctx, ref HookArgs.PlayerWorldAlteration args)
            {
                Vector2 Position = new Vector2(args.X, args.Y);

                if (args.TileWasPlaced && args.Type == SelectorItem && selection.isInSelectionlist(ctx.Player) && ctx.Player.Op)
                {
                    ctx.SetResult(HookResult.RECTIFY);
                    SelectorPos = !SelectorPos;

                    Vector2[] mousePoints = selection.GetSelection(ctx.Player);

                    if (!SelectorPos)
                        mousePoints[0] = Position;
                    else
                        mousePoints[1] = Position;

                    selection.SetSelection(ctx.Player, mousePoints);

                    ctx.Player.sendMessage(String.Format("You have selected block at {0},{1}, {2} position",
                        Position.X, Position.Y, (!SelectorPos) ? "First" : "Second"), ChatColor.Green);
                    return;
                }

                foreach (Region rgn in regionManager.Regions)
                {
                    if (rgn.HasPoint(Position))
                    {
                        if (IsRestrictedForUser(ctx.Player, rgn, ((args.TileWasRemoved || args.WallWasRemoved) ? TileBreak : TilePlace)))
                        {
                            ctx.SetResult(WorldAlter);
                            ctx.Player.sendMessage("You cannot edit this area!", ChatColor.Red);
                            return;
                        }
                    }
                }
            }

            [Hook(HookOrder.NORMAL)]
            void OnLiquidFlowReceived(ref HookContext ctx, ref HookArgs.LiquidFlowReceived args)
            {
                Vector2 Position = new Vector2(args.X, args.Y);

                foreach (Region rgn in regionManager.Regions)
                {
                    if (rgn.HasPoint(Position))
                    {
                        if (IsRestrictedForUser(ctx.Player, rgn, LiquidFlow))
                        {
                            ctx.SetResult(HookResult.ERASE);
                            ctx.Player.sendMessage("You cannot edit this area!", ChatColor.Red);
                            return;
                        }
                    }
                }
            }

            [Hook(HookOrder.NORMAL)]
            void OnProjectileReceived(ref HookContext ctx, ref HookArgs.ProjectileReceived args)
            {
                Vector2 Position = new Vector2(args.X, args.Y);

                foreach (Region rgn in regionManager.Regions)
                {
                    if (rgn.HasPoint(Position / 16))
                    {
                        if (rgn.ProjectileList.Contains("*") ||
                            rgn.ProjectileList.Contains(args.Type.ToString()))// ||
                            //rgn.ProjectileList.Contains(args.Projectile.Name.ToLower().Replace(" ", "")))
                        {
                            if (IsRestrictedForUser(ctx.Player, rgn, ProjectileUse))
                            {
                                ctx.SetResult(HookResult.ERASE);
                                ctx.Player.sendMessage("You cannot edit this area!", ChatColor.Red);
                                return;
                            }
                        }
                    }
                }
            }

            [Hook(HookOrder.NORMAL)]
            void OnDoorStateChange(ref HookContext ctx, ref HookArgs.DoorStateChanged args)
            {
                foreach (Region rgn in regionManager.Regions)
                {
                    if (rgn.HasPoint(new Vector2(args.X, args.Y)))
                    {
                        if (ctx.Sender is Player)
                        {
                            Player player = ctx.Sender as Player;
                            if (IsRestrictedForUser(player, rgn, DoorChange))
                            {
                                ctx.SetResult(HookResult.RECTIFY);
                                player.sendMessage("You cannot edit this area!", ChatColor.Red);
                                return;
                            }
                        }
                        else if (ctx.Sender is NPC)
                        {
                            if (rgn.RestrictedNPCs)
                            {
                                ctx.SetResult(HookResult.RECTIFY); //[TODO] look into RECIFYing for NPC's, They don't need to be resent, only cancelled, IGRNORE?
                                return;
                            }
                        } 
                    }
                }  
            }

            [Hook(HookOrder.NORMAL)]
            void OnChestBreak(ref HookContext ctx, ref HookArgs.ChestBreakReceived args)
            {
                foreach (Region rgn in regionManager.Regions)
                {
                    if (rgn.HasPoint(new Vector2(args.X, args.Y)))
                    {
                        if (ctx.Sender is Player)
                        {
                            if (IsRestrictedForUser(ctx.Player, rgn, DoorChange))
                            {
                                ctx.SetResult(HookResult.RECTIFY);
                                ctx.Player.sendMessage("You cannot edit this area!", ChatColor.Red);
                                return;
                            }
                        }
                    }
                }
            }

            [Hook(HookOrder.NORMAL)]
            void OnChestOpen(ref HookContext ctx, ref HookArgs.ChestOpenReceived args)
            {
                foreach (Region rgn in regionManager.Regions)
                {
                    if (rgn.HasPoint(new Vector2(args.X, args.Y)))
                    {
                        if (ctx.Sender is Player)
                        {
                            if (IsRestrictedForUser(ctx.Player, rgn, DoorChange))
                            {
                                ctx.SetResult(HookResult.RECTIFY);
                                ctx.Player.sendMessage("You cannot edit this object!", ChatColor.Red);
                                return;
                            }
                        }
                    }
                }
            }

            [Hook(HookOrder.NORMAL)]
            void OnSignEdit(ref HookContext ctx, ref HookArgs.SignTextSet args)
            {
                foreach (Region rgn in regionManager.Regions)
                {
                    if (rgn.HasPoint(new Vector2(args.X, args.Y)))
                    {
                        if (ctx.Sender is Player)
                        {
                            if (IsRestrictedForUser(ctx.Player, rgn, DoorChange))
                            {
                                ctx.SetResult(HookResult.IGNORE);
                                ctx.Player.sendMessage("You cannot edit this area!", ChatColor.Red);
                                return;
                            }
                        }
                    }
                }
            }
        #endregion

        public static bool IsRunningPermissions()
        {
            return Program.permissionManager.IsPermittedImpl != null;
        }

        public static bool IsRestricted(Node node, Player player)
        {
            if (IsRunningPermissions())
            {
                var isPermitted = Program.permissionManager.IsPermittedImpl(node.Path, player);
                var isOp = player.Op;

                return !isPermitted && !isOp;
            }

            return !player.Op;
        }

        public static bool IsRestrictedForUser(Player player, Region region, Node node)
        {
            if (UsingPermissions)
            {
                var Allowed = Program.permissionManager.IsPermittedImpl(node.Path, player);

                if (!Allowed)
                    return region.IsRestrictedForUser(player);

                return !Allowed;
            }

            return region.IsRestrictedForUser(player);
        }

        public void checkTable(string connectionString, string tablename)
        {
            bool isTable = false;
            //mysql- check to see if tables already exist
            IDbConnection dbcon;
            dbcon = new MySqlConnection(connectionString);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            string sql =
            " SHOW tables LIKE '" + tablename + "'";
            dbcmd.CommandText = sql;
            IDataReader reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                string checkresult = (string)reader["Tables_in_" + mysqldatabase + " (" + tablename + ")"];
                isTable = true;
            }
            ProgramLog.Plugin.Log("[Regions] checkTable " + tablename + ": Found:" + isTable);

            // clean up
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbcon.Close();
            dbcon = null;
            //end check

            if (!isTable)
            {
                //mysql- create table if not found
                string connectionString2 =
                "Server=" + mysqlserver + ";" +
                "Database=" + mysqldatabase + ";" +
                "User ID=" + mysqluser + ";" +
                "Password=" + mysqlpassword + ";" +
                "Pooling=false";
                IDbConnection dbcon2;
                dbcon2 = new MySqlConnection(connectionString2);
                dbcon2.Open();
                IDbCommand dbcmd2 = dbcon2.CreateCommand();
                string sql2 = "";
                if (tablename == "terraria_regions")
                {
                    sql2 = " CREATE TABLE " + tablename + " ( Name TEXT NOT NULL , Description TEXT NOT NULL, Point1 TEXT NOT NULL, Point2 TEXT NOT NULL, UserList TEXT NOT NULL, ProjectileList TEXT NOT NULL, Restricted BOOLEAN NOT NULL, RestrictedNPCs BOOLEAN NOT NULL ) ";
                }
                dbcmd2.CommandText = sql2;
                IDataReader reader2 = dbcmd2.ExecuteReader();
                // clean up
                reader2.Close();
                reader2 = null;
                dbcmd2.Dispose();
                dbcmd2 = null;
                dbcon2.Close();
                dbcon2 = null;
                ProgramLog.Plugin.Log("[Regions] Table '" + tablename + "' created.");
                //end create
            }
        }

    }
}
