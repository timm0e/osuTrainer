using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using osuTrainer.Commands;
using osuTrainer.Properties;
using ServiceStack.Text;

namespace osuTrainer.ViewModels
{
    public class OsuStatsViewModel : ScoresViewModel
    {
        public OsuStatsViewModel()
        {
            UpdateContent = "Update";
            LoadSettings();
            _client = new CustomWebClient();
            UpdateCommand = new UpdateCommand(this);
            OpenBeatmapLinkCommand = new OpenBeatmapLinkCommand(this);
            CopyLinkCommand = new CopyLinkCommand(this);
            DownloadCommand = new DownloadCommand(this);
            DownloadOdCommand = new DownloadOdCommand(this);
            DownloadBcCommand = new DownloadBcCommand(this);
        }

        private void LoadSettings()
        {
            Userid = Settings.Default.Userid;
            ApiKey = Settings.Default.ApiKey;
            MinPp = Settings.Default.MinPp;
            SelectedGameMode = Settings.Default.GameMode;
            IsDoubletimeCbChecked = Settings.Default.IsDoubletimeCbChecked;
            IsHardrockCbChecked = Settings.Default.IsHardrockCbChecked;
            IsHiddenCbChecked = Settings.Default.IsHiddenCbChecked;
            IsFlashlightCbChecked = Settings.Default.IsFlashlightCbChecked;
            IsFcOnlyCbChecked = Settings.Default.IsFcOnlyCbChecked;
            IsExclusiveCbChecked = Settings.Default.IsExclusiveCbChecked;
        }

        private bool GetUserBest()
        {
            UserScores = new List<int>();
            string json =
                _client.DownloadString(GlobalVars.UserApi + ApiKey + "&u=" + Userid + GlobalVars.Mode +
                                       SelectedGameMode);
            if (json.Length < 33)
            {
                return false;
            }
            Match match = Regex.Match(json, @"""user_id"":""(.+?)"".+?""username"":""(.+?)""");
            Userid = match.Groups[1].Value;

            json =
                _client.DownloadString(GlobalVars.UserBestApi + ApiKey + "&u=" + Userid + GlobalVars.Mode +
                                       SelectedGameMode);
            try
            {
                var userBest = JsonSerializer.DeserializeFromString<List<UserBest>>(json);
                foreach (UserBest item in userBest)
                {
                    UserScores.Add(item.Beatmap_Id);
                }
            }
            catch (Exception)
            {
            }

            try
            {
                json =
    _client.DownloadString(@"http://osustats.ezoweb.de/API/osuTrainer.php?mode=" + SelectedGameMode +
                           @"&uid=" + Userid);
                var osuStatsBest = JsonSerializer.DeserializeFromString<List<OsuStatsBest>>(json);
                foreach (OsuStatsBest item in osuStatsBest)
                {
                    UserScores.Add(item.Beatmap_Id);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        protected override ObservableCollection<ScoreInfo> GetScores()
        {
            IsWorking = true;
            UpdateContent = "Updating";
            var scores = new ObservableCollection<ScoreInfo>();
            if (!GetUserBest())
            {
                IsWorking = false;
                UpdateContent = "Update";
                MessageBox.Show("Wrong API key, username or the osustats API is down.");
                return scores;
            }
            string statsjson = "";
            string enabledMods = SelectedModsToString();
            try
            {
                statsjson =
                    _client.DownloadString(@"http://osustats.ezoweb.de/API/osuTrainer.php?mode=" + SelectedGameMode +
                                           @"&pp_value=" + MinPp + @"&mod_only_selected=" +
                                           IsExclusiveCbChecked.ToString().ToLowerInvariant() + @"&mod_string=" +
                                           enabledMods);
            }
            catch (Exception)
            {
                IsWorking = false;
                UpdateContent = "Update";
                MessageBox.Show("Failed to access the osustats API. Is osustats down?");
                return scores;
            }
            if (statsjson.Length < 3)
            {
                IsWorking = false;
                UpdateContent = "Update";
                MessageBox.Show("No scores found. Is osustats down?");
                return scores;
            }
            var osuStatsScores = JsonSerializer.DeserializeFromString<List<OsuStatsScores>>(statsjson);
            osuStatsScores =
                osuStatsScores.GroupBy(e => new { e.Beatmap_Id, e.Enabled_Mods }).Select(g => g.First()).ToList();
            for (int i = 0; i < osuStatsScores.Count; i++)
            {
                if (UserScores.Contains(osuStatsScores[i].Beatmap_Id)) continue;
                if (IsFcOnlyCbChecked) if ((int)osuStatsScores[i].Rank > 3) continue;
                osuStatsScores[i].Enabled_Mods &= ~(GlobalVars.Mods.NV | GlobalVars.Mods.Perfect | GlobalVars.Mods.SD | GlobalVars.Mods.SpunOut | GlobalVars.Mods.Autoplay);
                UserScores.Add(osuStatsScores[i].Beatmap_Id);
                double dtmodifier = 1.0;
                if (osuStatsScores[i].Enabled_Mods.HasFlag(GlobalVars.Mods.DT) ||
                    osuStatsScores[i].Enabled_Mods.HasFlag(GlobalVars.Mods.NC))
                {
                    dtmodifier = 1.5;
                }
                scores.Add(new ScoreInfo
                {
                    Accuracy = osuStatsScores[i].Accuracy,
                    Combo = osuStatsScores[i].MaxCombo,
                    Mods = osuStatsScores[i].Enabled_Mods,
                    BeatmapTitle = osuStatsScores[i].Beatmap_Title,
                    Version = osuStatsScores[i].Beatmap_Version,
                    BeatmapCreator = osuStatsScores[i].Beatmap_Creator,
                    BeatmapArtist = osuStatsScores[i].Beatmap_Artist,
                    Bpm = Math.Truncate(osuStatsScores[i].Beatmap_Bpm * dtmodifier),
                    Difficultyrating = Math.Round(osuStatsScores[i].Beatmap_Diffrating, 2),
                    Pp = Math.Truncate(osuStatsScores[i].Pp_Value),
                    TotalTime = TimeSpan.FromSeconds(osuStatsScores[i].Beatmap_Total_Length).ToString(@"mm\:ss"),
                    DrainingTime = TimeSpan.FromSeconds(osuStatsScores[i].Beatmap_Hit_Length).ToString(@"mm\:ss"),
                    RankImage = GetRankImageUri(osuStatsScores[i].Rank),
                    BeatmapId = osuStatsScores[i].Beatmap_Id,
                    BeatmapSetId = osuStatsScores[i].Beatmap_SetId,
                    ThumbUrl = GlobalVars.ThumbUrl + osuStatsScores[i].Beatmap_SetId + @"l.jpg",
                    ApprovedDate = osuStatsScores[i].Beatmap_RankedDate.ToShortDateString(),
                    Mode = "-"
                });
                ScoresAdded++;
            }
            IsWorking = false;
            UpdateContent = "Update";
            return scores;
        }
    }

    internal class OsuStatsScores
    {
        public string Username { get; set; }

        public int Uid { get; set; }
        public double Accuracy { get; set; }
        public int MaxCombo { get; set; }

        public GlobalVars.Mods Enabled_Mods { get; set; }

        public GlobalVars.Rank Rank { get; set; }

        public int Beatmap_Id { get; set; }

        public double Pp_Value { get; set; }

        public int Beatmap_SetId { get; set; }

        public double Beatmap_Diffrating { get; set; }

        public string Beatmap_Creator { get; set; }

        public double Beatmap_Bpm { get; set; }

        public string Beatmap_Title { get; set; }

        public string Beatmap_Artist { get; set; }

        public int Beatmap_Total_Length { get; set; }

        public int Beatmap_Hit_Length { get; set; }

        public string Beatmap_Version { get; set; }

        public DateTime Beatmap_RankedDate { get; set; }
    }

    internal class OsuStatsBest
    {
        public string Username { get; set; }

        public int Uid { get; set; }

        public GlobalVars.Mods Enabled_Mods { get; set; }

        public GlobalVars.Rank Rank { get; set; }

        public int Beatmap_Id { get; set; }

        public double PP_Value { get; set; }

        public int Beatmap_SetId { get; set; }

        public double Beatmap_Diffrating { get; set; }

        public string Beatmap_Creator { get; set; }

        public double Beatmap_BPM { get; set; }

        public string Beatmap_Title { get; set; }

        public string Beatmap_Artist { get; set; }

        public int Beatmap_Total_Length { get; set; }
    }
}