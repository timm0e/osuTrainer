﻿namespace osuTrainerOS
{
    public class OsuStatsScores
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

        public double Beatmap_Bpm { get; set; }

        public string Beatmap_Title { get; set; }

        public string Beatmap_Artist { get; set; }

        public int Beatmap_Total_Length { get; set; }

        public int Beatmap_Hit_Length { get; set; }

        public string Beatmap_Version { get; set; }
    }
}