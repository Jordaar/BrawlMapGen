﻿using System.Collections.Generic;

namespace BMG
{
    public class Options
    {

        public string setPath { get; set; }
        public string preset { get; set; }
        public BatchSettings[] batch { get; set; }
        public string exportFileName { get; set; } = "bmg_?number?.png";
        public string exportFolderName { get; set; } = "output";
        public bool saveLogFile { get; set; } = true;
        public ConsoleOptions console { get; set; }
        public Title title { get; set; }

        public class Replace
        {
            public char from { get; set; }
            public char to { get; set; }
        }

        public class BatchSettings
        {
            public string name { get; set; } = "?number?";
            public string[] map { get; set; }
            public int biome { get; set; }
            public int sizeMultiplier { get; set; }
            public char[] skipTiles { get; set; }
            public Replace[] replaceTiles { get; set; }
            public string exportFileName { get; set; }
            public Tiledata.TileDefault[] overrideBiome { get; set; }
            public SpecialTileRules[] specialTileRules { get; set; }
            public float[] emptyBorderAmount { get; set; } = new float[] { 1 };
            public string gamemode { get; set; }
        }

        public class ConsoleOptions
        {
            public bool setup { get; set; } = true;
            public bool tileDraw { get; set; } = true;
            public bool orderedHorTileDraw { get; set; } = true;
            public bool orderedTileDraw { get; set; } = true;
            public bool saveLocation { get; set; } = true;
            public bool aal { get; set; } = true;
            public bool statusChange { get; set; } = true;
            public bool gamemodeModding { get; set; } = true;
        }

        public class SpecialTileRules
        {
            public char tileCode { get; set; }
            public int tileTime { get; set; }
            public int tileType { get; set; }
        }

        public class RecordedSTR
        {
            public char tileCode { get; set; }
            public int tileTime { get; set; }
        }

        public static void RecordRSTR(List<RecordedSTR> rstrArray, char tileCode)
        {
            foreach (var rstro in rstrArray)
                if (rstro.tileCode == tileCode)
                {
                    rstro.tileTime++;
                    return;
                }

            rstrArray.Add(new RecordedSTR()
            {
                tileCode = tileCode,
                tileTime = 1
            });
        }

        public class AppInfo
        {
            public bool showVersion { get; set; } = true;
        }

        public class Job
        {
            public char percentageBarFillCharacter { get; set; } = '#';
            public char percentageBarBackgroundCharacter { get; set; } = '-';
            public string order { get; set; } = "?percentage? [?progressBar?] ?jobName? ?jobsRatio?";
        }

        public class Status
        {
            public char percentageBarFillCharacter { get; set; } = '#';
            public char percentageBarBackgroundCharacter { get; set; } = '-';
            public string order { get; set; } = "?percentage? [?progressBar?] ?statusText? ?actionRatio?";
        }

        public class StatusDetails
        {
            public bool showBiome { get; set; } = true;
            public bool showTile { get; set; } = true;
        }

        public class Modules
        {
            public AppInfo appInfo { get; set; } = new AppInfo();
            public Job job { get; set; } = new Job();
            public Status status { get; set; } = new Status();
            public StatusDetails statusDetails { get; set; } = new StatusDetails();
        }

        public class Title
        {
            public Modules modules { get; set; } = new Modules();
            public string layout { get; set; } = "?appInfo? - ?job? - ?status? - ?statusDetails?";
            public bool disableUpdate { get; set; } = false;
        }

    }

}
