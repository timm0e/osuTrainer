using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using osuTrainer.Properties;
using osuTrainer.ViewModels;

namespace osuTrainer.Views
{
    /// <summary>
    ///     Interaction logic for Scores.xaml
    /// </summary>
    public partial class Scores : UserControl
    {
        public Scores()
        {
            InitializeComponent();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Userid = UseridTb.Text;
            Settings.Default.ApiKey = PasswordBox.Password;
            Settings.Default.MinPp = PpSlider.Value;
            Settings.Default.GameMode = GameModeCb.SelectedIndex;
            Settings.Default.IsDoubletimeCbChecked = DoubletimeCb.IsChecked ?? false;
            Settings.Default.IsHardrockCbChecked = HardrockCb.IsChecked ?? false;
            Settings.Default.IsHiddenCbChecked = HiddenCb.IsChecked ?? false;
            Settings.Default.IsFlashlightCbChecked = FlashlightCb.IsChecked ?? false;
            Settings.Default.IsFcOnlyCbChecked = FcOnlyCb.IsChecked ?? false;
            Settings.Default.IsExclusiveCbChecked = ExclusiveCb.IsChecked ?? false;
            Settings.Default.Save();
            if (ScoreSourceCb.SelectedIndex != Settings.Default.DataSource)
            {
                switch (ScoreSourceCb.SelectedIndex)
                {
                    case 1:
                        DataContext = new OsuStatsViewModel();
                        break;
                    default:
                        DataContext = new OsuApiViewModel();
                        break;
                }
            }
            Settings.Default.DataSource = ScoreSourceCb.SelectedIndex;
            Settings.Default.Save();
        }

        private void ApiLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Scores_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext == null)
            {
                switch (Settings.Default.DataSource)
                {
                    case 1:
                        DataContext = new OsuStatsViewModel();
                        break;
                    default:
                        DataContext = new OsuApiViewModel();
                        break;
                }
            }
            PasswordBox.Password = Settings.Default.ApiKey;
        }

        private void Scores_Initialized(object sender, EventArgs e)
        {
            GameModeCb.ItemsSource = Enum.GetValues(typeof (GlobalVars.GameMode)).Cast<int>();
            ScoreSourceCb.Items.Add("osu! API");
            ScoreSourceCb.Items.Add("osustats API");
            ScoreSourceCb.SelectedIndex = Settings.Default.DataSource;
        }
    }
}