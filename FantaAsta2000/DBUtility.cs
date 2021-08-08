using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FantaAsta2000.DataConstructs;
using static System.Net.Mime.MediaTypeNames;

namespace FantaAsta2000
{
    public class DBUtility
    {
        SQLiteConnection connection;

        public DBUtility()
        {
            string relativePath = @"Database\FantacalcioDB.sqlite";
            string myString = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string absolutePath = Path.Combine(Path.GetDirectoryName(myString), relativePath);
            if (!Directory.Exists(Path.GetDirectoryName(absolutePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            string connectionString = string.Format("Data Source={0};Version=3;Pooling=True;Max Pool Size=100;", absolutePath);
            if (!File.Exists(absolutePath))
                SQLiteConnection.CreateFile(absolutePath);
            connection = new SQLiteConnection();
            connection.ConnectionString = connectionString;
            connection.Open();
        }

        public void ResetDatabase()
        {
            SQLiteCommand command = new SQLiteCommand();

            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = "DROP TABLE IF EXISTS Players;";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE TABLE Players (idPlayer INTEGER NOT NULL PRIMARY KEY, idFantacalcio INTEGER NOT NULL, role VARCHAR(4) NOT NULL, mantraRole VARCHAR(64) NOT NULL, playerName VARCHAR(64) NOT NULL, playerTeam VARCHAR(32) NOT NULL, originalPlayerValue INT NOT NULL, finalPlayerValue INT NOT NULL, sold BIT NOT NULL, idCoach INT NOT NULL);";
                command.ExecuteNonQuery();
                command.CommandText = "DROP TABLE IF EXISTS Coaches;";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE TABLE Coaches (idCoach INTEGER NOT NULL PRIMARY KEY, CoachName VARCHAR(64) NOT NULL, totGoalKeepers INTEGER NOT NULL," +
                    " goalKeepersExpense INTEGER NOT NULL, totDefenders INTEGER NOT NULL, defendersExpense INTEGER NOT NULL, totMidfielders INTEGER NOT NULL" +
                    ", midfieldersExpense INTEGER NOT NULL, totStrikers INTEGER NOT NULL, strikersExpense INTEGER NOT NULL, remainingFunds INTEGER NOT NULL);";
                command.ExecuteNonQuery();
            }
            catch (Exception ex) { throw new Exception(ex.Message, ex); }
            finally { command.Dispose(); }
        }

        public Configuration GetConfiguration(string leagueName)
        {
            //SQLiteConnection connection = new SQLiteConnection();
            SQLiteCommand command = new SQLiteCommand();
            Configuration config = null;

            try
            {
                //connection.ConnectionString = connectionString;
                //connection.Open();
                command = new SQLiteCommand(connection);
                if (TableConfigurationExist())
                {
                    command.CommandText = "SELECT * FROM ConfigurationLeague WHERE leagueName = '" + leagueName + "'";
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        config = new Configuration();
                        config.LeagueName = leagueName;
                        config.LeagueType = (string)reader["leagueType"];
                        config.Funds = Convert.ToInt32(reader["funds"]);
                        config.MaxDefenders = Convert.ToInt32(reader["maxDefenders"]);
                        config.MaxGoalKeepers = Convert.ToInt32(reader["maxGoalKeepers"]);
                        config.MaxMidfielders = Convert.ToInt32(reader["maxMidfielders"]);
                        config.MaxStrikers = Convert.ToInt32(reader["maxStrikers"]);
                        config.MaxPlayersRose = Convert.ToInt32(reader["maxPlayersRose"]);
                    }
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message, ex); }
            finally
            {
                command.Dispose();
                //connection.Dispose();
            }

            return config;
        }

        public List<Player> GetPlayers()
        {
            SQLiteCommand command = new SQLiteCommand();
            List<Player> players = new List<Player>();

            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = "SELECT * FROM Players WHERE sold = 0";
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Player player = new Player();
                    player.Id = Convert.ToInt32(reader["idPlayer"]);
                    player.IdFantacalcio = Convert.ToInt32(reader["idFantacalcio"]);
                    player.Role = (string)reader["role"];
                    player.RoleMantra = (string)reader["mantraRole"];
                    player.Name = (string)reader["playerName"];
                    player.Sold = Convert.ToBoolean(reader["sold"]);
                    player.Team = (string)reader["playerTeam"];
                    player.Value = Convert.ToInt32(reader["originalPlayerValue"]);
                    players.Add(player);
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message, ex); }
            finally
            {
                command.Dispose();
                //connection.Dispose();
            }

            return players;
        }

        public List<CsvPlayer> GetPlayersWithTeams()
        {
            SQLiteCommand command = new SQLiteCommand();
            List<CsvPlayer> players = new List<CsvPlayer>();

            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = "DROP VIEW IF EXISTS Extract;";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE VIEW Extract AS SELECT Coaches.CoachName, Players.idFantacalcio, Players.finalPlayerValue, Players.idCoach FROM Players LEFT OUTER JOIN Coaches ON Players.idCoach = Coaches.idCoach WHERE Players.idCoach > 0";
                command.ExecuteNonQuery();
                command.CommandText = "SELECT * FROM Extract ORDER BY idCoach";
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    CsvPlayer player = new CsvPlayer();
                    player.Coachname = (string)reader["CoachName"];
                    player.IdFantaCalcio = Convert.ToInt32(reader["idFantacalcio"]);
                    player.FinalPlayerValue = Convert.ToInt32(reader["finalPlayerValue"]);
                    players.Add(player);
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message, ex); }
            finally
            {
                command.Dispose();
                //connection.Dispose();
            }

            return players;
        }

        public List<Coach> GetCoaches()
        {
            //SQLiteConnection connection = new SQLiteConnection();
            SQLiteCommand command = new SQLiteCommand();
            List<Coach> coaches = new List<Coach>();

            try
            {
                //connection.ConnectionString = connectionString;
                //connection.Open();
                command = new SQLiteCommand(connection);
                command.CommandText = "SELECT * FROM Coaches";
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Coach coach = new Coach();
                    coach.Id = Convert.ToInt32(reader["idCoach"]);
                    coach.Name = (string)reader["coachName"];
                    coach.TotGoalKeepers = Convert.ToInt32(reader["totGoalKeepers"]);
                    coach.GoalKeepersExpense = Convert.ToInt32(reader["goalKeepersExpense"]);
                    coach.TotDefenders = Convert.ToInt32(reader["totDefenders"]);
                    coach.DefendersExpense = Convert.ToInt32(reader["defendersExpense"]);
                    coach.TotMidfielders = Convert.ToInt32(reader["totMidfielders"]);
                    coach.MidfieldersExpense = Convert.ToInt32(reader["midfieldersExpense"]);
                    coach.TotStrikers = Convert.ToInt32(reader["totStrikers"]);
                    coach.StrikersExpense = Convert.ToInt32(reader["strikersExpense"]);
                    coach.RemainingFunds = Convert.ToInt32(reader["remainingFunds"]);
                    coaches.Add(coach);
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message, ex); }
            finally
            {
                command.Dispose();
                //connection.Dispose();
            }

            return coaches;
        }

        public void UpdateSoldPlayers(int idPlayer, int idCoach, int buyingPrice)
        {
            SQLiteCommand command = new SQLiteCommand();

            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = "UPDATE Players SET sold = 1, idCoach = " + idCoach + ", finalPlayerValue = " + buyingPrice + " WHERE idPlayer = " + idPlayer;
                command.ExecuteNonQuery();
            }
            catch (Exception ex) { throw new Exception(ex.Message, ex); }
            finally
            {
                command.Dispose();
                //connection.Dispose();
            }
        }

        public void UpdateCoaches(int idCoach, int goalKeepersExpense = 0, int defendersExpense = 0, int midfieldersExpense = 0, int strikersExpense = 0)
        {
            SQLiteCommand command = new SQLiteCommand();
            string query = "UPDATE Coaches SET ";

            try
            {
                if (goalKeepersExpense != 0)
                    query += " totGoalKeepers = totGoalKeepers + 1, goalKeepersExpense = goalKeepersExpense + " + goalKeepersExpense + ", remainingFunds = remainingFunds - " + goalKeepersExpense;
                if (defendersExpense != 0)
                    query += " totDefenders = totDefenders + 1, defendersExpense = defendersExpense + " + defendersExpense + ", remainingFunds = remainingFunds - " + defendersExpense;
                if (midfieldersExpense != 0)
                    query += " totMidfielders = totMidfielders + 1, midfieldersExpense = midfieldersExpense + " + midfieldersExpense + ", remainingFunds = remainingFunds - " + midfieldersExpense;
                if (strikersExpense != 0)
                    query += " totStrikers = totStrikers + 1, strikersExpense = strikersExpense + " + strikersExpense + ", remainingFunds = remainingFunds - " + strikersExpense;
                //connection.ConnectionString = connectionString;
                //connection.Open();
                command = new SQLiteCommand(connection);
                command.CommandText = query + " WHERE idCoach = " + idCoach;
                command.ExecuteNonQuery();
            }
            catch (Exception ex) { throw new Exception(ex.Message, ex); }
            finally
            {
                command.Dispose();
                //connection.Dispose();
            }
        }

        public void PopulateTablePlayers(List<Player> players)
        {
            SQLiteCommand command = new SQLiteCommand();

            try
            {
                command = new SQLiteCommand(connection);
                players.ForEach(player =>
                {
                    command.CommandText = "INSERT INTO Players (idPlayer, idFantacalcio, role, mantraRole, playerName, playerTeam, originalPlayerValue, finalPlayerValue, sold, idCoach) VALUES(" + String.Format("{0},{1},'{2}','{3}','{4}','{5}','{6}', -1,{7}, -1", player.Id, player.IdFantacalcio, player.Role, player.RoleMantra, player.Name.Replace("'","''"), player.Team, player.Value, false) + ")";
                    command.ExecuteNonQuery();
                });
            }
            catch (Exception ex) { throw new Exception(ex.Message, ex); }
            finally
            {
                command.Dispose();
                //connection.Dispose();
            }
        }

        public void PopulateTableCoaches(List<Coach> coaches)
        {
           // SQLiteConnection connection = new SQLiteConnection();
            SQLiteCommand command = new SQLiteCommand();

            try
            {
                //connection.ConnectionString = connectionString;
                //connection.Open();
                command = new SQLiteCommand(connection);
                coaches.ForEach(coach =>
                {
                    command.CommandText = "INSERT INTO Coaches (idCoach, CoachName, totGoalKeepers, goalKeepersExpense, totDefenders, defendersExpense, totMidfielders, midfieldersExpense, totStrikers, strikersExpense, remainingFunds) VALUES(" + String.Format("{0},'{1}', 0, 0, 0, 0, 0, 0, 0, 0, {2}", coach.Id, coach.Name, coach.RemainingFunds) + ")";
                    command.ExecuteNonQuery();
                });
            }
            catch (Exception ex) { throw new Exception(ex.Message, ex); }
            finally
            {
                command.Dispose();
                //connection.Dispose();
            }
        }

        public void InsertOrUpdateTableConfiguration(Configuration config)
        {
            //SQLiteConnection connection = new SQLiteConnection();
            SQLiteCommand command = new SQLiteCommand();

            try
            {
                if (String.IsNullOrWhiteSpace(config.LeagueName))
                    throw new Exception("Nome lega vuoto.");

                //connection.ConnectionString = connectionString;
                //connection.Open();
                command = new SQLiteCommand(connection);
                if (TableConfigurationExist())
                {
                    command.CommandText = "SELECT * FROM ConfigurationLeague WHERE leagueName = '" + config.LeagueName + "'";
                    var res = command.ExecuteScalar();
                    if (res != null)
                    {
                        // Update
                        command.CommandText = "UPDATE ConfigurationLeague " +
                            "SET leagueType = '" + config.LeagueType + "', funds = " + config.Funds + ", maxGoalKeepers = " + config.MaxGoalKeepers + ", maxDefenders = " + config.MaxDefenders + ", maxMidfielders = " + config.MaxMidfielders + ", maxStrikers = " + config.MaxStrikers + ", maxPlayersRose = " + config.MaxPlayersRose +
                            " WHERE leagueName = '" + config.LeagueName + "'";
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        // Insert
                        command.CommandText = "INSERT INTO ConfigurationLeague (leagueName, leagueType, funds, maxGoalKeepers, maxDefenders, maxMidfielders, maxStrikers, maxPlayersRose) " +
                            "VALUES(" + String.Format("'{0}','{1}', {2}, {3}, {4}, {5}, {6}, {7}", config.LeagueName, config.LeagueType, config.Funds, config.MaxGoalKeepers, config.MaxDefenders, config.MaxMidfielders, config.MaxStrikers, config.MaxPlayersRose) + ")";
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    // Create AND Insert
                    command.CommandText = "CREATE TABLE ConfigurationLeague (leagueName VARCHAR(32) NOT NULL PRIMARY KEY, leagueType VARCHAR(8) NOT NULL, funds INTEGER NOT NULL, maxGoalKeepers INTEGER NOT NULL, maxDefenders INTEGER NOT NULL, maxMidfielders INTEGER NOT NULL, maxStrikers INTEGER NOT NULL, maxPlayersRose INTEGER NOT NULL);";
                    command.ExecuteNonQuery();
                    command.CommandText = "INSERT INTO ConfigurationLeague (leagueName, leagueType, funds, maxGoalKeepers, maxDefenders, maxMidfielders, maxStrikers, maxPlayersRose) " +
                        "VALUES(" + String.Format("'{0}','{1}', {2}, {3}, {4}, {5}, {6}, {7}", config.LeagueName, config.LeagueType, config.Funds, config.MaxGoalKeepers, config.MaxDefenders, config.MaxMidfielders, config.MaxStrikers, config.MaxPlayersRose) + ")";
                    command.ExecuteNonQuery();
                }

            }
            catch (Exception ex) { throw new Exception(ex.Message, ex); }
            finally
            {
                command.Dispose();
                //connection.Dispose();
            }
        }

        private bool TableConfigurationExist()
        {
            //SQLiteConnection connection = new SQLiteConnection();
            SQLiteCommand command = new SQLiteCommand();

            try
            {
                //connection.ConnectionString = connectionString;
                //connection.Open();
                command = new SQLiteCommand(connection);
                command.CommandText = "SELECT * FROM sqlite_master WHERE name ='ConfigurationLeague' and type='table';";
                var result = command.ExecuteReader();
                if (result.HasRows)
                    return true;
            }
            catch (Exception ex) { throw new Exception(ex.Message, ex); }
            finally
            {
                command.Dispose();
                //connection.Dispose();
            }

            return false;
        }
    }
}
