using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using static FantaAsta2000.DataConstructs;

namespace FantaAsta2000
{
    public partial class MainWindow : Window
    {
        Configuration config;
        DBUtility dbUtility;
        string pathPlayers;
        string pathTeams;

        public MainWindow()
        {
            InitializeComponent();
            
            dbUtility = new DBUtility();

            pathPlayers = "";
            pathTeams = "";

            List<string> leaguesList = new List<string>() { "ARCHIVA", "BTF" };
            cb_LeagueName.ItemsSource = leaguesList;
        }

        private void Btn_NextPage_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)ckb_NewAuction.IsChecked)
            {
                if (File.Exists(pathPlayers))
                    config.PathPlayers = pathPlayers;
                else
                {
                    MessageBox.Show("E' selezionata una 'Nuova Asta' è necessario quindi caricare la 'lista dei giocatori'!", "Finestra per poveri allocchi", MessageBoxButton.OK);
                    return;
                }
                if (File.Exists(pathTeams))
                    config.PathTeams = pathTeams;
                else
                {
                    MessageBox.Show("E' selezionata una 'Nuova Asta' è necessario quindi caricare la 'lista delle squadre'!", "Finestra per poveri allocchi", MessageBoxButton.OK);
                    return;
                }

                config.NewAuction = true;
            }
            else
                config.NewAuction = false;

            if(!(bool)rb_Classic.IsChecked && !(bool)rb_Mantra.IsChecked)
            {
                MessageBox.Show("Selezionare la modalità della lega!", "Finestra per poveri allocchi", MessageBoxButton.OK);
                return;
            }

            Main.Content = new ConfigurationUI(config, dbUtility);
        }

        private void Btn_Browse_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a Label
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                pathPlayers = filename;
                lb_PathPlayers.Content = System.IO.Path.GetFileName(filename);
                AutoClosingMessageBox.Show("Lista Giocatori Caricata!", "Info", 500);
            }
        }

        private void Btn_BrowseTeamName_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a Label
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                pathTeams = filename;
                lb_PathTeams.Content = System.IO.Path.GetFileName(filename);
                AutoClosingMessageBox.Show("Lista Squadre Caricata!", "Info", 500);
            }
        }

        private void cb_LeagueName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_LeagueName.SelectedValue == null)
                return;

            ckb_NewAuction.Visibility = Visibility.Visible;
            lb_Mode.Visibility = Visibility.Visible;
            rb_Classic.Visibility = Visibility.Visible;
            rb_Mantra.Visibility = Visibility.Visible;
            Btn_BrowseTeamName.Visibility = Visibility.Visible;
            lb_PathTeams.Visibility = Visibility.Visible;
            Btn_Browse.Visibility = Visibility.Visible;
            lb_PathPlayers.Visibility = Visibility.Visible;
            Btn_NextPage.Visibility = Visibility.Visible;

            ckb_NewAuction.IsChecked = false;
            ckb_NewAuction.IsEnabled = true;
            config = dbUtility.GetConfiguration((string)cb_LeagueName.SelectedValue);
            if(config != null)
            {
                if (config.LeagueType.ToUpper() == "MANTRA")
                    rb_Mantra.IsChecked = true;
                else
                    rb_Classic.IsChecked = true;
            }
            else
            {
                config = new Configuration();
                config.LeagueName = cb_LeagueName.SelectedValue.ToString();
                config.Funds = -1;
                config.MaxPlayersRose = -1;
                ckb_NewAuction.IsChecked = true;
                ckb_NewAuction.IsEnabled = false;
            }
        }

        private void rb_Classic_Checked(object sender, RoutedEventArgs e)
        {
            rb_Mantra.IsChecked = false;
            config.LeagueType = "CLASSIC";
        }

        private void rb_Mantra_Checked(object sender, RoutedEventArgs e)
        {
            rb_Classic.IsChecked = false;
            config.LeagueType = "MANTRA";
        }
    }

    public class AutoClosingMessageBox
    {
        System.Threading.Timer _timeoutTimer;
        string _caption;
        AutoClosingMessageBox(string text, string caption, int timeout)
        {
            _caption = caption;
            _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                null, timeout, System.Threading.Timeout.Infinite);
            MessageBox.Show(text, caption);
        }

        public static void Show(string text, string caption, int timeout)
        {
            new AutoClosingMessageBox(text, caption, timeout);
        }

        void OnTimerElapsed(object state)
        {
            IntPtr mbWnd = FindWindow(null, _caption);
            if (mbWnd != IntPtr.Zero)
                SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            _timeoutTimer.Dispose();
        }
        const int WM_CLOSE = 0x0010;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
    }
}
