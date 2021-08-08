using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static FantaAsta2000.DataConstructs;

namespace FantaAsta2000
{
    /// <summary>
    /// Interaction logic for ConfigurationUI.xaml
    /// </summary>
    public partial class ConfigurationUI : Page
    {
        Configuration config = null;
        DBUtility dbUtilityConfig = null;

        public ConfigurationUI(Configuration config, DBUtility dbUtility)
        {
            InitializeComponent();

            this.config = config;
            this.dbUtilityConfig = dbUtility;
            if (this.config != null)
            {
                Budget.Text = this.config.Funds.ToString();
                NumberMaxPlayers.Text = this.config.MaxPlayersRose.ToString();
                if (this.config.MaxPlayersRose != -1)
                    ckb_freeRose.IsChecked = true;
                else
                {
                    ckb_freeRose.IsChecked = false;
                    NumberMaxPlayers.Text = "-1";
                    NumberMaxGolkeeper.Text = this.config.MaxGoalKeepers.ToString();
                    NumberMaxDefender.Text = this.config.MaxDefenders.ToString();
                    NumberMaxMid.Text = this.config.MaxMidfielders.ToString();
                    NumberMaxStriker.Text = this.config.MaxStrikers.ToString();
                }
            }
        }

        private void ckb_freeRose_Checked(object sender, RoutedEventArgs e)
        {
            NumberMaxPlayers.Visibility = Visibility.Visible;
            lb_MaxPlayers.Visibility = Visibility.Visible;
            lb_MaxGolkeeper.Visibility = Visibility.Hidden;
            NumberMaxGolkeeper.Visibility = Visibility.Hidden;
            lb_MaxDefender.Visibility = Visibility.Hidden;
            NumberMaxDefender.Visibility = Visibility.Hidden;
            lb_MaxMid.Visibility = Visibility.Hidden;
            NumberMaxMid.Visibility = Visibility.Hidden;
            lb_MaxStriker.Visibility = Visibility.Hidden;
            NumberMaxStriker.Visibility = Visibility.Hidden;
        }

        private void ckb_freeRose_Unchecked(object sender, RoutedEventArgs e)
        {
            NumberMaxPlayers.Visibility = Visibility.Hidden;
            lb_MaxPlayers.Visibility = Visibility.Hidden;
            lb_MaxGolkeeper.Visibility = Visibility.Visible;
            NumberMaxGolkeeper.Visibility = Visibility.Visible;
            lb_MaxDefender.Visibility = Visibility.Visible;
            NumberMaxDefender.Visibility = Visibility.Visible;
            lb_MaxMid.Visibility = Visibility.Visible;
            NumberMaxMid.Visibility = Visibility.Visible;
            lb_MaxStriker.Visibility = Visibility.Visible;
            NumberMaxStriker.Visibility = Visibility.Visible;
        }

        private void Btn_NextPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)ckb_freeRose.IsChecked)
                {
                    config.MaxGoalKeepers = -1;
                    config.MaxDefenders = -1;
                    config.MaxMidfielders = -1;
                    config.MaxStrikers = -1;
                    if (IsPositiveInt(NumberMaxPlayers.Text) && Convert.ToInt32(NumberMaxPlayers.Text) > 0)
                        config.MaxPlayersRose = Convert.ToInt32(NumberMaxPlayers.Text);
                    else
                        throw new Exception();
                }
                else
                {
                    if (IsPositiveInt(NumberMaxGolkeeper.Text))
                        config.MaxGoalKeepers = Convert.ToInt32(NumberMaxGolkeeper.Text);
                    else
                        throw new Exception();
                    if (IsPositiveInt(NumberMaxGolkeeper.Text))
                        config.MaxDefenders = Convert.ToInt32(NumberMaxDefender.Text);
                    else
                        throw new Exception();
                    if (IsPositiveInt(NumberMaxGolkeeper.Text))
                        config.MaxMidfielders = Convert.ToInt32(NumberMaxMid.Text);
                    else
                        throw new Exception();
                    if (IsPositiveInt(NumberMaxGolkeeper.Text))
                        config.MaxStrikers = Convert.ToInt32(NumberMaxStriker.Text);
                    else
                        throw new Exception();
                    config.MaxPlayersRose = -1;
                }
            }
            catch
            {
                MessageBox.Show("Numeri dei giocatori che devono essere in Rosa non validi!", "Finestra per poveri allocchi", MessageBoxButton.OK);
                return;
            }

            try
            {
                if (IsPositiveInt(Budget.Text))
                {
                    config.Funds = Convert.ToInt32(Budget.Text);
                    if (config.Funds < 1)
                        throw new Exception();
                }
                else
                    throw new Exception();
            }
            catch
            {
                MessageBox.Show("Budget per le rose non valido! Selezionare un numero maggiore di 0.", "Finestra per poveri allocchi", MessageBoxButton.OK);
                return;
            }

            dbUtilityConfig.InsertOrUpdateTableConfiguration(config);

            if(config.NewAuction)
            {
                // Reset del DB
                MessageBoxResult resReset = MessageBox.Show("ATTENZIONE :: Hai selezionato 'Nuova Asta' questo comporta un RESET del DB! Confermi?", "Conferma", MessageBoxButton.YesNo);
                if (resReset == MessageBoxResult.Yes)
                {
                    MessageBoxResult resResetSure = MessageBox.Show("ATTENZIONE :: SEI SICURO di voler Resettare il DB???", "Conferma", MessageBoxButton.YesNo);
                    if (resResetSure == MessageBoxResult.Yes)
                    {
                        dbUtilityConfig.ResetDatabase();
                        List<Player> players = new List<Player>();
                        try
                        {
                            players.AddRange(getAllRealPlayers(config.PathPlayers));
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Finestra per poveri allocchi", MessageBoxButton.OK);
                            return;
                        }
                        List<Coach> coaches = new List<Coach>();
                        coaches.AddRange(getAllCoaches(config.PathTeams));
                        dbUtilityConfig.PopulateTablePlayers(players);
                        dbUtilityConfig.PopulateTableCoaches(coaches);
                    }
                    else
                        return;
                }
                else
                    return;
            }

            Main.Content = new RandomizeUI(config, dbUtilityConfig);
        }

        private List<Player> getAllRealPlayers(string playersPath)
        {
            List<Player> players = new List<Player>();
            int id = 0;

            List<String> playersFromFile = File.ReadAllLines(playersPath).ToList();
            playersFromFile.ForEach(x =>
            {
                string[] row = x.Split(';');

                Player player = new Player();
                player.Id = id;
                if(!IsPositiveInt(row[0]))
                    throw new Exception("Nel file dei Giocatori alla riga " + id + 1 + " non è presente un intero positivo nella prima colonna.");
                player.IdFantacalcio = Convert.ToInt32(row[0]);
                player.Role = row[1];
                player.RoleMantra = row[6];
                player.Name = row[2];
                player.Sold = bool.Parse(row[5]);
                player.Team = row[3];
                player.Value = int.Parse(row[4]);
                players.Add(player);

                id++;
            });

            return players;
        }

        public List<Coach> getAllCoaches(string coachesPath)
        {
            List<Coach> coachesList = new List<Coach>();
            coachesList.Add(new Coach() { Id = 0, Name = "FAKETEAM", RemainingFunds = 10000 });
            List<string> list = ((IEnumerable<string>)File.ReadAllLines(coachesPath)).ToList<string>();
            for (int index = 1; index <= list.Count<string>(); ++index)
            {
                coachesList.Add(new Coach()
                {
                    Id = index,
                    Name = list[index-1],
                    RemainingFunds = config.Funds
                });
            }
            return coachesList;
        }

        private bool IsPositiveInt(string value)
        {
            try
            {
                int tempIntValue = Convert.ToInt32(value);
                if (tempIntValue < 0)
                    throw new Exception();
            }
            catch { return false; }

            return true;
        }
    }
}
