using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantaAsta2000
{
    public class DataConstructs
    {
        public class Player
        {
            public int Id { get; set; }
            public int IdFantacalcio { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }
            public string RoleMantra { get; set; }
            public string Team { get; set; }
            public int Value { get; set; }
            public bool Sold { get; set; }
        }

        public class Coach
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int TotGoalKeepers { get; set; }
            public int GoalKeepersExpense { get; set; }
            public int TotDefenders { get; set; }
            public int DefendersExpense { get; set; }
            public int TotMidfielders { get; set; }
            public int MidfieldersExpense { get; set; }
            public int TotStrikers { get; set; }
            public int StrikersExpense { get; set; }
            public int RemainingFunds { get; set; }
        }

        public class Configuration
        {
            public string LeagueName { get; set; }
            public string LeagueType { get; set; }
            public bool NewAuction { get; set; }
            public string PathPlayers { get; set; }
            public string PathTeams { get; set; }
            public int Funds { get; set; }
            public int MaxGoalKeepers { get; set; }
            public int MaxDefenders { get; set; }
            public int MaxMidfielders { get; set; }
            public int MaxStrikers { get; set; }
            public int MaxPlayersRose { get; set; }
        }

        public class CsvPlayer
        {
            public int IdFantaCalcio { get; set; }
            public string Coachname { get; set; }
            public int FinalPlayerValue { get; set; }
        }
    }
}
