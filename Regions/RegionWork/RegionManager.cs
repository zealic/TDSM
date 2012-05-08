using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria_Server.Logging;
using System.IO;
using Terraria_Server.Misc;
using System.Data;

using MySql.Data.MySqlClient;

namespace Regions.RegionWork
{
    public class RegionManager
    {
        public List<Region> RegionList { get; set; }
        private string SaveFolder { get; set; }

        public RegionManager(string saveFolder)
        {
            SaveFolder = saveFolder;
			RegionList = new List<Region>();

            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);
        }

        public void LoadRegions()
        {
            ProgramLog.Plugin.Log("Loading Regions.");
            if (Regions.instance.mysqlenabled)
            {
                RegionList = LoadMysqlRegions();
            }
            else
            {
                RegionList = LoadRegions(SaveFolder);
            }
            ProgramLog.Plugin.Log("Loaded {0} Regions.", RegionList.Count);
        }

        public int import()
        {
            List<Region> Regions = LoadRegions(SaveFolder);
            int count = 0;
            foreach (Region r in Regions)
            {
                SaveRegion(r);
                count++;
            }
            return count;
        }

        public bool SaveRegion(Region region)
        {
            FileStream fs = null;
            try
            {
                if (region != null && region.IsValidRegion())
                {
                    if (Regions.instance.mysqlenabled)
                    {
                        string regionname = region.Name.Replace("'", @"\'");
                        string regiondesc = region.Description.Replace("'", @"\'");	
                        string point1 = region.Point1.X + "," + region.Point1.Y;
                        string point2 = region.Point2.X + "," + region.Point2.Y;
                        string userList = region.UserListToString().Replace("'", @"\'");	
                        string projectileList = region.ProjectileListToString().Replace("'", @"\'");	
                        string sql = "";
                        IDbConnection dbcon;
                        dbcon = new MySqlConnection(Regions.instance.connectionString);
                        dbcon.Open();
                        IDbCommand dbcmd = dbcon.CreateCommand();
                        if (getMysqlRegion(regionname) == null)
                        {
                            sql = "INSERT INTO terraria_regions (Name, Description, Point1, Point2, UserList, ProjectileList, Restricted, RestrictedNPCs) VALUES ('" + regionname + "', '" + regiondesc + "', '" + point1 + "', '" + point2 + "', '" + userList.Trim() + "', '" + projectileList.Trim() + "', " + region.Restricted + ", " + region.RestrictedNPCs + ")";
                        }
                        else
                        {
                            sql = "UPDATE terraria_regions SET Description = '" + regiondesc + "', Point1 = '" + point1 + "' , Point2 = '" + point2 + "', UserList = '" + userList + "', ProjectileList = '" + projectileList + "', Restricted = "+region.Restricted+", RestrictedNPCs = "+region.RestrictedNPCs+" WHERE Name = '" + regionname + "' ";
                        }
                        dbcmd.CommandText = sql;
                        IDataReader reader = dbcmd.ExecuteReader();
                        // clean up
                        reader.Close();
                        reader = null;
                        dbcmd.Dispose();
                        dbcmd = null;
                        dbcon.Close();
                        dbcon = null;

                        return true;
                    }
                    else
                    {
                        string file = SaveFolder + Path.DirectorySeparatorChar + region.Name + ".rgn";

                        if (File.Exists(file))
                            File.Delete(file);

                        fs = File.Open(file, FileMode.CreateNew);
                        string toWrite = region.ToString();
                        fs.Write(ASCIIEncoding.ASCII.GetBytes(toWrite), 0, toWrite.Length);
                        fs.Flush();
                        fs.Close();
                        return true;
                    }
                }
                else
                    ProgramLog.Error.Log("Region '{0}' was either null or has an issue.",
                        (region != null && region.Name != null) ? region.Name : "??");
            }
            catch (Exception e)
            {
                ProgramLog.Error.Log("Error saving Region {0}\n{1}", region.Name, e.Message);
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
            return false;
        }

        private string getMysqlRegion(string name)
        {
            string rname = null;
            IDbConnection dbcon;
            dbcon = new MySqlConnection(Regions.instance.connectionString);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            string sql = "SELECT * FROM terraria_regions WHERE Name = '" + name + "'";
            dbcmd.CommandText = sql;
            IDataReader reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                rname = (string)reader["Name"];
            }
            // clean up
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbcon.Close();
            dbcon = null;
            return rname;
        }

        private Region LoadRegion(string location)
        {
            Region region = new Region();

            string Name = "";
            string Description = "";
            Vector2 Point1 = default(Vector2);
            Vector2 Point2 = default(Vector2);
            List<String> Users = new List<String>();
            List<String> Projectiles = new List<String>();
            bool Restricted = false;
            bool RestrictedNPCs = false;

            foreach (string line in File.ReadAllLines(location))
            {
                if (line.Contains(":"))
                {
                    string key = line.Split(':')[0];
                    switch (key)
                    {
                        case "name":
                            {
                                Name = line.Remove(0, line.IndexOf(":") + 1).Trim();
                                break;
                            }
                        case "description":
                            {
                                Description = line.Remove(0, line.IndexOf(":") + 1).Trim();
                                break;
                            }
                        case "point1":
                            {
                                string[] xy = line.Remove(0, line.IndexOf(":") + 1).Trim().Split(',');
                                float x, y;
                                if (!(float.TryParse(xy[0], out x) && float.TryParse(xy[1], out y)))
                                    Point1 = default(Vector2);
                                else
                                    Point1 = new Vector2(x, y);
                                break;
                            }
                        case "point2":
                            {
                                string[] xy = line.Remove(0, line.IndexOf(":") + 1).Trim().Split(',');
                                float x, y;
                                if (!(float.TryParse(xy[0], out x) && float.TryParse(xy[1], out y)))
                                    Point2 = default(Vector2);
                                else
                                    Point2 = new Vector2(x, y);
                                break;
                            }
                        case "users":
                            {
                                string userlist = line.Remove(0, line.IndexOf(":") + 1).Trim();
                                if(userlist.Length > 0)
                                    Users = userlist.Split(' ').ToList<String>();
                                break;
                            }
                        case "projectiles":
                            {
                                string userlist = line.Remove(0, line.IndexOf(":") + 1).Trim();
                                if(userlist.Length > 0)
                                    Projectiles = userlist.Split(' ').ToList<String>();
                                break;
                            }
                        case "restricted":
                            {
                                string restricted = line.Remove(0, line.IndexOf(":") + 1).Trim();
                                bool restrict;
                                if (Boolean.TryParse(restricted, out restrict))
                                    Restricted = restrict;
                                break;
                            }
                        case "npcrestrict":
                            {
                                string restricted = line.Remove(0, line.IndexOf(":") + 1).Trim();
                                bool restrict;
                                if (Boolean.TryParse(restricted, out restrict))
                                    RestrictedNPCs = restrict;
                                break;
                            }
                        default: continue;
                    }
                }
            }

            region.Name = Name;
            region.Description = Description;
            region.Point1 = Point1;
            region.Point2 = Point2;
            region.UserList = Users;
            region.ProjectileList = Projectiles;
            region.Restricted = Restricted;
            region.RestrictedNPCs = RestrictedNPCs;

            return region.IsValidRegion() ? region : null;
        }

        private List<Region> LoadRegions(string folder)
        {
            List<Region> rgns = new List<Region>();
            foreach (string file in Directory.GetFiles(folder))
            {
                if (file.ToLower().EndsWith(".rgn"))
                {
                    Region rgn = LoadRegion(file);
                    if (rgn != null)
                        rgns.Add(rgn);
                }
            }
            return rgns;
        }

        private List<Region> LoadMysqlRegions()
        {
            List<Region> regions = new List<Region>();
            Region region;
            Vector2 Point1;
            Vector2 Point2;
            IDbConnection dbcon;
            dbcon = new MySqlConnection(Regions.instance.connectionString);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            string sql = "SELECT * FROM terraria_regions";
            dbcmd.CommandText = sql;
            IDataReader reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                region = new Region();
                Point1 = default(Vector2);
                Point2 = default(Vector2);
                region.Name = (string)reader["Name"];
                region.Description = (string)reader["Description"];

                string line = (string)reader["Point1"];
                string[] xy = line.Split(',');
                float x, y;
                if (!(float.TryParse(xy[0], out x) && float.TryParse(xy[1], out y)))
                    Point1 = default(Vector2);
                else
                    Point1 = new Vector2(x, y);

                region.Point1 = Point1;

                string line2 = (string)reader["Point2"];
                string[] xy2 = line2.Split(',');
                float x2, y2;
                if (!(float.TryParse(xy2[0], out x2) && float.TryParse(xy2[1], out y2)))
                    Point2 = default(Vector2);
                else
                    Point2 = new Vector2(x2, y2);

                region.Point2 = Point2;
                string userlist = (string)reader["UserList"];
                region.UserList = userlist.Split(' ').ToList<String>();
                string projlist = (string)reader["ProjectileList"];
                region.ProjectileList = projlist.Split(' ').ToList<String>();
                region.Restricted = (bool)reader["Restricted"];
                region.RestrictedNPCs = (bool)reader["RestrictedNPCs"];

                if(region.IsValidRegion())
                    regions.Add(region);
            }
            // clean up
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbcon.Close();
            dbcon = null;

            return regions;
        }

        public bool ContainsRegion(string name)
        {
            foreach (Region rgn in RegionList)
            {
                if (rgn.Name.ToLower().Equals(name.ToLower()))
                    return true;
            }

            return false;
        }

        public Region GetRegion(string name)
        {
            foreach (Region rgn in RegionList)
            {
                if (rgn.Name.ToLower().Equals(name.ToLower()))
                    return rgn;
            }

            return null;
        }
    }
}
