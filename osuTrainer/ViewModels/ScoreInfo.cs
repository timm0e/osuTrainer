using System;

namespace osuTrainer.ViewModels
{
    public class ScoreInfo
    {
        public double Accuracy { get; set; }
        public int Combo { get; set; }
        public string RankImage { get; set; }

        public string Version { get; set; }

        public GlobalVars.Mods Mods { get; set; }
        public double Bpm { get; set; }

        public double Pp { get; set; }

        public int BeatmapId { get; set; }
        public int BeatmapSetId { get; set; }
        public double Difficultyrating { get; set; }
        public string BeatmapTitle { get; set; }
        public string BeatmapArtist { get; set; }
        public string BeatmapCreator { get; set; }
        public string ThumbUrl { get; set; }
        public string TotalTime { get; set; }
        public string DrainingTime { get; set; }
        public string ApprovedDate { get; set; }
        public int DiffSize { get; set; }
        public int DiffOverall { get; set; }
        public int DiffApproach { get; set; }
        public int DiffDrain { get; set; }
        public string Mode { get; set; }
    }
}