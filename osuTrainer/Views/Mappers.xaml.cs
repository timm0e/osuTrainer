using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Windows.Threading;
using MahApps.Metro.Controls;
using osuTrainer.Properties;
using osuTrainer.ViewModels;
using ServiceStack.Text;
using Path = System.IO.Path;

namespace osuTrainer.Views
{
    /// <summary>
    /// Interaction logic for Mappers.xaml
    /// </summary>
    public partial class Mappers : UserControl
    {
        private readonly CustomWebClient _client = new CustomWebClient();
        private SortedSet<BeatmapInfo> _maps = new SortedSet<BeatmapInfo>();
        private readonly string _textfile = "mappers.txt";
        private readonly BackgroundWorker _worker = new BackgroundWorker();
        private List<int> _mappers = new List<int>();
        private DispatcherTimer _timer = new DispatcherTimer();

        public Mappers()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var editor = new Editor(_textfile);
            editor.Owner = Application.Current.MainWindow;
            editor.ShowDialog();
            if (editor.Saved)
            {
                LoadMappers();
                timer_Tick(sender, e);
            }
        }

        private void Mappers_OnInitialized(object sender, EventArgs e)
        {
            _timer.Interval = new TimeSpan(0, 10, 47);
            _timer.Start();
            _timer.Tick += timer_Tick;
            _worker.DoWork += WorkerOnDoWork;
            _worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
            LoadMappers();
            timer_Tick(sender, e);
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            foreach (int mapper in _mappers)
            {
                string json = "";
                try
                {
                    json = _client.DownloadString(GlobalVars.BeatmapApi + Settings.Default.ApiKey + "&u=" +
                           mapper);
                }
                catch (Exception)
                {    
                }
                if (json.Length < 33) continue;
                var test = JsonSerializer.DeserializeFromString<SortedSet<BeatmapInfo>>(json);
                foreach (var item in test)
                {
                    _maps.Add(new BeatmapInfo
                    {
                        Beatmapset_Id = item.Beatmapset_Id,
                        Beatmap_Id = item.Beatmap_Id,
                        Approved = item.Approved,
                        Approved_Date = item.Approved_Date,
                        Last_Update = item.Last_Update - new TimeSpan(8, 0, 0),
                        Total_Length = item.Total_Length,
                        Hit_Length = item.Hit_Length,
                        Version = item.Version,
                        Artist = item.Artist,
                        Title = item.Title,
                        Creator = item.Creator,
                        Bpm = item.Bpm,
                        Source = item.Source,
                        Difficultyrating = item.Difficultyrating,
                        Diff_Size = item.Diff_Size,
                        Diff_Overall = item.Diff_Overall,
                        Diff_Approach = item.Diff_Approach,
                        Diff_Drain = item.Diff_Drain,
                        Mode = item.Mode,
                        UserId = mapper
                    });
                }
            }
        }

        private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            MappersSp.Children.Clear();
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            int row = 0;
            foreach (BeatmapInfo item in _maps)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });

                var link = new Hyperlink();
                link.NavigateUri = new Uri(GlobalVars.UserUrl + item.UserId);
                link.RequestNavigate += LinkOnRequestNavigate;
                link.Inlines.Add(new Run(item.Creator));

                var text = new TextBlock();
                text.Inlines.Add(link);
                text.Inlines.Add(" updated ");

                link = new Hyperlink();
                link.NavigateUri = new Uri(GlobalVars.BeatmapUrl + item.Beatmap_Id);
                link.RequestNavigate += LinkOnRequestNavigate;
                link.Inlines.Add(new Run(item.Artist + " - " + item.Title + " [" + item.Version + "]"));

                text.Inlines.Add(link);
                text.Inlines.Add(" (" + item.Mode + ")");
                grid.Children.Add(text);
                Grid.SetRow(text, row);
                Grid.SetColumn(text, 1);

                text = new TextBlock();
                text.Text = ElapsedDate(item.Last_Update);
                grid.Children.Add(text);
                Grid.SetRow(text, row);
                Grid.SetColumn(text, 0);

                row++;
            }
            MappersSp.Children.Add(grid);
        }

        private string ElapsedDate(DateTime date)
        {
            TimeSpan elapsed = DateTime.Now.ToUniversalTime() - date;
            if (elapsed.Days > 0)
            {
                if (elapsed.Days > 1)
                {
                    return elapsed.Days + " Days ago";
                }
                return elapsed.Days + " Day ago";
            }
            if (elapsed.Hours > 0)
            {
                if (elapsed.Hours > 1)
                {
                    return elapsed.Hours + " Hours ago";
                }
                return elapsed.Hours + " Hour ago";
            }
            if (elapsed.Minutes > 1)
            {
                return elapsed.Minutes + " Minutes ago";
            }
            return elapsed.Minutes + " Minute ago";
        }

        private void LinkOnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

        private void timer_Tick(object sender, EventArgs eventArgs)
        {
            if (!_worker.IsBusy && _mappers.Count > 0)
            {
                var progressRing = new ProgressRing();
                progressRing.IsActive = true;
                progressRing.HorizontalAlignment = HorizontalAlignment.Left;
                MappersSp.Children.Clear();
                MappersSp.Children.Add(progressRing);
                _maps.Clear();
                _worker.RunWorkerAsync();
            }
        }

        private void LoadMappers()
        {
            if (!File.Exists(_textfile)) CreateMappers();
            using (var reader = new StreamReader(_textfile))
            {
                _mappers = new List<int>();
                while (true)
                {
                    string line;
                    if ((line = reader.ReadLine()) == null) break;
                    var userid = 0;
                    int.TryParse(line, out userid);
                    if (userid > 0)
                    {
                        try
                        {
                            _mappers.Add(Convert.ToInt32(line));
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Failed to read this line: " + line);
                            return;
                        }
                    }

                }
            }
        }

        private void CreateMappers()
        {
            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "mappers.txt"), Properties.Resources.mappers);
        }
    }

    internal class BeatmapInfo : IComparable<BeatmapInfo>
    {
        public int Beatmapset_Id { get; set; }
        public int Beatmap_Id { get; set; }
        public int Approved { get; set; }
        public DateTime? Approved_Date { get; set; }
        public DateTime Last_Update { get; set; }
        public int Total_Length { get; set; }
        public int Hit_Length { get; set; }
        public string Version { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Creator { get; set; }
        public int Bpm { get; set; }
        public string Source { get; set; }
        public double Difficultyrating { get; set; }
        public double Diff_Size { get; set; }
        public double Diff_Overall { get; set; }
        public double Diff_Approach { get; set; }
        public double Diff_Drain { get; set; }
        public GlobalVars.GameMode Mode { get; set; }
        public int UserId { get; set; }

        public int CompareTo(BeatmapInfo other)
        {
            if (other.Equals(this))
                return 0;

            return other.Last_Update.CompareTo(Last_Update);
        }
    }
}
