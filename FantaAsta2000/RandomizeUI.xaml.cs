using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using static FantaAsta2000.DataConstructs;

namespace FantaAsta2000
{
    /// <summary>
    /// Interaction logic for RandomizeUI.xaml
    /// </summary>
    public partial class RandomizeUI : Page
    {
        List<Player> players = null;
        List<Player> playersForSearch = null;
        List<Player> rbPlayers = null;
        List<Coach> coaches = null;
        Player selectedPlayer = null;
        RadioButton buttonChecked = null;
        Coach selectedCoach = null;
        DBUtility dbUtilityRandom = null;
        int buyingPrice = 0;
        Configuration confRose;

        private static Guid FolderDownloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);

        public RandomizeUI(Configuration conf, DBUtility dbUtility)
        {
            InitializeComponent();
            
            players = new List<Player>();
            playersForSearch = new List<Player>();
            rbPlayers = new List<Player>();
            coaches = new List<Coach>();
            selectedPlayer = new Player();
            confRose = conf;
            dbUtilityRandom = dbUtility;

            players.AddRange(dbUtility.GetPlayers());
            playersForSearch = players;
            coaches.AddRange(dbUtility.GetCoaches());
            coachCb.ItemsSource = coaches.Select(x => x.Name).ToList();
            AutoCompletePlayer.ItemsSource = playersForSearch;
        }

        public List<Player> playersRandomizer(List<Player> playersDividedByRole)
        {
            Random rdm = new Random();
            return playersDividedByRole.OrderBy<Player, int>((Func<Player, int>)(x => rdm.Next())).ToList<Player>();
        }

        public bool checkTheMoney(Coach owner, int buyingPrice) => owner.RemainingFunds - buyingPrice > 0;

        private void Randomize_Click(object sender, RoutedEventArgs e)
        {
            if (playersLeftLb.Content.Equals("0"))
            {
                MessageBox.Show("Ue' Bro! Non hai giocatori a questa Quotazione!", "Finestra per poveri allocchi", MessageBoxButton.OK);
                return;
            }

            if (quotMinCb.SelectedValue == null)
            {
                MessageBox.Show("Ue' Bro! Seleziona un valore minimo per i giocatori!", "Finestra per poveri allocchi", MessageBoxButton.OK);
                return;
            }
            if (buttonChecked == null)
            {
                MessageBox.Show("Ue' Bro! Seleziona il ruolo dei giocatori!", "Finestra per poveri allocchi", MessageBoxButton.OK);
                return;
            }

            rbPlayers = players.Where(x => buttonChecked.Name.Equals(x.Role) && x.Sold == false && x.Value >= Int32.Parse(quotMinCb.SelectedValue.ToString())).ToList();

            if (rbPlayers.Count == 0)
            {
                MessageBox.Show("Ue' Bro! Hai finito i giocatori per questa fascia di prezzo! (LA FIERA E' FINITA!!! (cit.))", "Finestra per poveri allocchi", MessageBoxButton.OK);
                return;
            }

            // chiamata randomize
            populateSelectedPlayerTBoxes(playersRandomizer(rbPlayers).First());

        }

        private void quotMinCb_Selected(object sender, RoutedEventArgs e)
        {
            if (quotMinCb.SelectedValue == null)
                playersLeftLb.Content = players.Where(x => buttonChecked.Name.Equals(x.Role) && x.Sold == false).Count().ToString();
            else
                playersLeftLb.Content = players.Where(x => buttonChecked.Name.Equals(x.Role) && x.Sold == false && x.Value >= Int32.Parse(quotMinCb.SelectedValue.ToString())).Count().ToString();
        }

        private void RbClick(object sender, RoutedEventArgs e)
        {
            buttonChecked = MainRandomize.Children.OfType<RadioButton>().Where(x => x.GroupName == "rbGroup" && (bool)x.IsChecked).Single();
            rbPlayers = players.Where(x => buttonChecked.Name.Equals(x.Role) && x.Sold == false).ToList();
            playersLeftLb.Content = rbPlayers.Count().ToString();
            cleanUpValues();
        }

        private void cleanUpValues()
        {
            playerTb.Text = String.Empty;
            teamTb.Text = String.Empty;
            originalMoneyTb.Text = String.Empty;
            roleMantraTb.Text = String.Empty;
            quotMinCb.Text = String.Empty;
            quotMinCb.ItemsSource = rbPlayers.Select(x => x.Value).Distinct().OrderByDescending(x => x).ToList();
        }

        private void populateSelectedPlayerTBoxes(Player player)
        {
            AutoCompletePlayer.Text = String.Empty;
            selectedPlayer = player;
            playerTb.Text = player.Name;
            teamTb.Text = player.Team;
            originalMoneyTb.Text = player.Value.ToString();
            roleMantraTb.Text = player.RoleMantra.ToString();
        }

        private void deleteSelectedPlayerTBoxes()
        {
            playerTb.Text = String.Empty;
            teamTb.Text = String.Empty;
            originalMoneyTb.Text = String.Empty;
            roleMantraTb.Text = String.Empty;
        }

        private void coachCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (coachCb.SelectedValue == null)
                return;
            Coach selectedCoach = coaches.Where(x => x.Name.Equals(coachCb.SelectedValue.ToString())).First();
            updateCoachStatusLabel(selectedCoach);
        }

        private void updateCoachStatusLabel(Coach selectedCoach)
        {
            coachOverviewLb.Content = String.Format("Coach: {0}/ P: {1}/ D: {2}/ C: {3}/ A: {4}/ M: {5}", selectedCoach.Name, selectedCoach.TotGoalKeepers, selectedCoach.TotDefenders, selectedCoach.TotMidfielders, selectedCoach.TotStrikers, selectedCoach.RemainingFunds);
        }

        private void resetCoachCboxAndPrice()
        {
            coachCb.SelectedValue = String.Empty;
            buyPriceTb.Text = "1";
        }

        private void Sold_Click(object sender, RoutedEventArgs e)
        {
            if (String.Empty.Equals(coachCb.SelectedValue) || coachCb.SelectedValue == null)
            {
                MessageBox.Show("Ue' Bro! Scegli un ALLENATORE se non ti è di disturbo!", "Finestra per poveri allocchi", MessageBoxButton.OK);
                return;
            }

            try
            {
                buyingPrice = Int32.Parse(buyPriceTb.Text);
                selectedCoach = coaches.Where(x => x.Name.Equals(coachCb.SelectedValue.ToString())).First();

                if (!checkTheMoney(selectedCoach, buyingPrice))
                {
                    MessageBox.Show(String.Format("Sembra che {0} non abbia i soldi per questo giocatore! Che Colley-One!", selectedCoach.Name), "Finestra per poveri allocchi", MessageBoxButton.OK);
                    return;
                }

                // Se il calciatore venduto non è nella lista rbPlayers (quindi Count = 0) si tratta di un giocatore ricercato manualmente quindi lo cerco nella lista di tutti i calciatori
                Player soldPlayer = null;
                if (rbPlayers.Count(x => x.Id == selectedPlayer.Id) == 0)
                    soldPlayer = players.Where(x => x.Id == selectedPlayer.Id).First();
                else
                    soldPlayer = rbPlayers.Where(x => x.Id == selectedPlayer.Id).First();

                switch (soldPlayer.Role)
                {
                    case "P":
                        if (confRose.MaxGoalKeepers > 0 && selectedCoach.TotGoalKeepers >= confRose.MaxGoalKeepers && coachCb.SelectedValue.ToString().ToUpper() != "FAKETEAM")
                        {
                            MessageBox.Show("Hai raggiunto il numero massimo di giocatori per questa categoria!", "Finestra per poveri allocchi", MessageBoxButton.OK);
                            return;
                        }
                        selectedCoach.GoalKeepersExpense = selectedCoach.GoalKeepersExpense + buyingPrice;
                        selectedCoach.TotGoalKeepers++;
                        dbUtilityRandom.UpdateCoaches(selectedCoach.Id, buyingPrice);
                        break;
                    case "D":
                        if (confRose.MaxDefenders > 0 && selectedCoach.TotDefenders >= confRose.MaxDefenders && coachCb.SelectedValue.ToString().ToUpper() != "FAKETEAM")
                        {
                            MessageBox.Show("Hai raggiunto il numero massimo di giocatori per questa categoria!", "Finestra per poveri allocchi", MessageBoxButton.OK);
                            return;
                        }
                        selectedCoach.DefendersExpense = selectedCoach.DefendersExpense + buyingPrice;
                        selectedCoach.TotDefenders++;
                        dbUtilityRandom.UpdateCoaches(selectedCoach.Id, 0, buyingPrice);
                        break;
                    case "C":
                        if (confRose.MaxMidfielders > 0 && selectedCoach.TotMidfielders >= confRose.MaxMidfielders && coachCb.SelectedValue.ToString().ToUpper() != "FAKETEAM")
                        {
                            MessageBox.Show("Hai raggiunto il numero massimo di giocatori per questa categoria!", "Finestra per poveri allocchi", MessageBoxButton.OK);
                            return;
                        }
                        selectedCoach.MidfieldersExpense = selectedCoach.MidfieldersExpense + buyingPrice;
                        selectedCoach.TotMidfielders++;
                        dbUtilityRandom.UpdateCoaches(selectedCoach.Id, 0, 0, buyingPrice);
                        break;
                    case "A":
                        if (confRose.MaxStrikers > 0 && selectedCoach.TotStrikers >= confRose.MaxStrikers && coachCb.SelectedValue.ToString().ToUpper() != "FAKETEAM")
                        {
                            MessageBox.Show("Hai raggiunto il numero massimo di giocatori per questa categoria!", "Finestra per poveri allocchi", MessageBoxButton.OK);
                            return;
                        }
                        selectedCoach.StrikersExpense = selectedCoach.StrikersExpense + buyingPrice;
                        selectedCoach.TotStrikers++;
                        dbUtilityRandom.UpdateCoaches(selectedCoach.Id, 0, 0, 0, buyingPrice);
                        break;
                    default:
                        MessageBox.Show(String.Format("Sembra che {0} non abbia un ruolo ben definito.......", selectedPlayer.Name), "Finestra per poveri allocchi", MessageBoxButton.OK);
                        return;
                }

                // Verifico di non aver sforato il numero massimo di giocatori (se devo)
                if(confRose.MaxPlayersRose > 0 && coachCb.SelectedValue.ToString().ToUpper() != "FAKETEAM" && (selectedCoach.TotGoalKeepers + selectedCoach.TotDefenders + selectedCoach.TotMidfielders + selectedCoach.TotStrikers) >= confRose.MaxPlayersRose)
                {
                    MessageBox.Show("Hai raggiunto il numero massimo di giocatori in Rosa!", "Finestra per poveri allocchi", MessageBoxButton.OK);
                    return;
                }

                selectedCoach.RemainingFunds = selectedCoach.RemainingFunds - buyingPrice;
                // Segno il giocatore come venduto
                rbPlayers.Where(x => x.Id == selectedPlayer.Id).ToList().ForEach(x => x.Sold = true);
                // Aggiorno il db con il giocatore acquistato in caso di crash riparto da questa situazione
                dbUtilityRandom.UpdateSoldPlayers(soldPlayer.Id, selectedCoach.Id, buyingPrice);
                // Se non sto usando il FAKETEAM, rimuovo dalla lista per la 
                // ricerca il giocatore venduto. Altrimenti nella ricerca lascio 
                // la possibilità di ricercare anche un giocatore scartato.
                if (coachCb.SelectedValue.ToString().ToUpper() != "FAKETEAM")
                {
                    playersForSearch.Remove(soldPlayer);
                    // Ricarico l'Autocomplete di ricerca
                    AutoCompletePlayer.ItemsSource = new List<Player>();
                    AutoCompletePlayer.ItemsSource = playersForSearch;
                }
                playersLeftLb.Content = rbPlayers.Where(x => x.Sold == false).Count().ToString();

                if (rbPlayers.Where(x => x.Sold == false).Count() > 0)
                    populateSelectedPlayerTBoxes(playersRandomizer(rbPlayers.Where(x => x.Sold == false).ToList()).First());
                else
                {
                    deleteSelectedPlayerTBoxes();
                    quotMinCb.SelectedValue = String.Empty;
                    MessageBox.Show("Finiti i giocatori per il ruolo selezionato!", "Finestra per poveri allocchi", MessageBoxButton.OK);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Finestra per poveri allocchi", MessageBoxButton.OK); }
            finally
            {
                updateCoachStatusLabel(selectedCoach);
                resetCoachCboxAndPrice();
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            List<CsvPlayer> recsForCsv = dbUtilityRandom.GetPlayersWithTeams();
            if(recsForCsv == null || recsForCsv.Count == 0)
            {
                MessageBox.Show("Non c'è nemmeno un giocatore assegnato ad una ROSA!", "Finestra per poveri allocchi", MessageBoxButton.OK);
                return;
            }
            List<string> csv = new List<string>();
            string currentCoach = recsForCsv.First().Coachname;
            // Create Csv in Memory
            csv.Add("$,$,$");
            recsForCsv.ForEach(rec =>
            {
                if (currentCoach.ToUpper() != rec.Coachname.ToUpper())
                {
                    csv.Add("$,$,$");
                    currentCoach = rec.Coachname;
                }
                csv.Add(rec.Coachname + "," + rec.IdFantaCalcio + "," + rec.FinalPlayerValue);
            });

            string dwnPath = System.IO.Path.Combine(GetDownloadsPath(), "Rose_" + confRose.LeagueName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv");
            File.WriteAllLines(dwnPath, csv);
            MessageBox.Show("File scaricato in: " + dwnPath);
        }

        private string GetDownloadsPath()
        {
            if (Environment.OSVersion.Version.Major < 6) throw new NotSupportedException();
            IntPtr pathPtr = IntPtr.Zero;

            try
            {
                SHGetKnownFolderPath(ref FolderDownloads, 0, IntPtr.Zero, out pathPtr);
                return Marshal.PtrToStringUni(pathPtr);
            }
            finally { Marshal.FreeCoTaskMem(pathPtr); }
        }

        private void AutoCompletePlayer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                LaunchToRandomize();
        }

        private void AutoCompletePlayer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LaunchToRandomize();
        }

        private void LaunchToRandomize()
        {
            var item = AutoCompletePlayer.SelectedItem;
            Player p = (Player)item;
            if (p == null)
                return;
            populateSelectedPlayerTBoxes(playersForSearch.First(x => x.Id == p.Id));
        }
    }
}
