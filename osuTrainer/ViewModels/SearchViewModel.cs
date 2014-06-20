using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using osuTrainer.Commands;
using osuTrainer.Properties;
using ServiceStack.Text;

namespace osuTrainer.ViewModels
{
    class SearchViewModel : ViewModelBase
    {
        protected CustomWebClient _client = new CustomWebClient();
        private ObservableCollection<BeatmapScoreDisplay> _scores = new ObservableCollection<BeatmapScoreDisplay>();
        public ObservableCollection<BeatmapScoreDisplay> Scores
        {
            get { return _scores; }
            set
            {
                _scores = value;
                RaisePropertyChanged("Scores");
            }
        }
        private bool _isWorking;
        public bool IsWorking
        {
            get { return _isWorking; }
            set
            {
                if (Equals(value, _isWorking))
                {
                    return;
                }
                _isWorking = value;
                RaisePropertyChanged("IsWorking");
            }
        }
        public int SelectedGameMode { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand CallSearchCommand { get; set; }

        public string BeatmapId { get; set; }

        public SearchViewModel()
        {
            SearchCommand = new SearchCommand(this);
        }
        public async void GetScoresAsync()
        {
            Scores = await Task.Run(() => GetScores()).ConfigureAwait(false);
        }
        private ObservableCollection<BeatmapScoreDisplay> GetScores()
        {
            IsWorking = true;
            var json = _client.DownloadString(GlobalVars.ScoresApi + Settings.Default.ApiKey + "&b=" + BeatmapId + "&m="+SelectedGameMode);
            if (json.Length < 33)
            {
                IsWorking = false;
                MessageBox.Show("Wrong API key or Beatmap Id.\nMake sure to use not to use the Beatmapset Id!");
                return null;
            }
            var scores = JsonSerializer.DeserializeFromString<ObservableCollection<BeatmapScore>>(json);
            var display = new ObservableCollection<BeatmapScoreDisplay>();
            foreach (var item in scores)
            {
                display.Add(new BeatmapScoreDisplay
                {
                    RankImage = GetRankImageUri(item.Rank),
                    Accuracy = Math.Round(
                            GetAccuracy(item.Count50, item.Count100, item.Count300,
                                item.CountMiss, item.CountKatu, item.CountGeki), 2),
                    Player = item.Username,
                    MaxCombo = item.MaxCombo,
                    CountMiss = item.CountMiss,
                    EnabledMods = item.Enabled_Mods,
                    Pp = Math.Round(item.Pp,2)
                });
            }
            IsWorking = false;
            return display;
        }
        protected double GetAccuracy(int count50, int count100, int count300, int countmiss, int countkatu, int countgeki)
        {
            // https://osu.ppy.sh/wiki/Accuracy
            switch (SelectedGameMode)
            {
                case 0:
                    return (float)(count50 * 50 + count100 * 100 + count300 * 300) / (countmiss + count50 + count100 + count300) / 3;
                case 1:
                    return (float)(count100 * .5 + count300) / (countmiss + count100 + count300) * 100;
                case 2:
                    return (float)(count300 + count100 + count50) /
                           (count300 + count100 + count50 + countkatu + countmiss) * 100;
                case 3:
                    return (float)(count50 * 50 + count100 * 100 + countkatu * 200 + count300 * 300 + countgeki * 300) /
                           (countmiss + count50 + count100 + countkatu + count300 + countgeki) / 3;
                default:
                    return 0.0;
            }
        }
    }

    internal class BeatmapScoreDisplay
    {
        public string RankImage { get; set; }
        public double Accuracy { get; set; }
        public string Player { get; set; }
        public int MaxCombo { get; set; }
        public int CountMiss { get; set; }
        public GlobalVars.Mods EnabledMods { get; set; }
        public double Pp { get; set; }
    }

    internal class BeatmapScore
    {
        public int Score { get; set; }
        public string Username { get; set; }
        public int Count300 { get; set; }
        public int Count100 { get; set; }
        public int Count50 { get; set; }
        public int CountMiss { get; set; }
        public int MaxCombo { get; set; }
        public int CountKatu { get; set; }
        public int CountGeki { get; set; }
        public int Perfect { get; set; }
        public GlobalVars.Mods Enabled_Mods { get; set; }
        public int User_Id { get; set; }
        public DateTime Date { get; set; }
        public GlobalVars.Rank Rank { get; set; }
        public double Pp { get; set; }
    }
}
