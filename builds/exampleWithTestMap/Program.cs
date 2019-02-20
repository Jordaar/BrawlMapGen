﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using Svg;
using System.IO;
using Newtonsoft.Json;
using System.Threading;

namespace generator
{
    class Program
    {
        static void Main(string[] args)
        {

            if (!File.Exists("options.json"))
                Environment.Exit(0);

            StreamReader r = new StreamReader("options.json");
            string json = r.ReadToEnd();
            var options = JsonConvert.DeserializeObject<Options>(json);

            if (options.console.setup)
                Console.WriteLine("Brawl Map Gen v1.3\nCreated by RedH1ghway aka TheDonciuxx\nWith the help of 4JR\n\nLoading " + options.preset + " preset");

            if (options.console.aal)
                Console.WriteLine("[ AAL ] READ << presets\\" + options.preset + ".json");

            if (!File.Exists("presets\\" + options.preset + ".json"))
            {
                Console.WriteLine("\nERROR:\nPreset doesn't exist\n[FileReader] Unable to find file in location \"presets\\" + options.preset + ".json\"");
                Thread.Sleep(3000);
                Environment.Exit(1);
            }

            StreamReader r2 = new StreamReader("presets\\" + options.preset + ".json");
            string json2 = r2.ReadToEnd();
            var tiledata = JsonConvert.DeserializeObject<Tiledata>(json2);

            if (options.console.setup)
                Console.WriteLine("Preset \"" + options.preset.ToUpper() + "\" loaded.");

            int bNumber = 0;
            foreach (var batchOption in options.batch)
            {
                bNumber++;

                if (options.console.setup)
                    Console.WriteLine("Reading the map in the index number " + bNumber + "...");

                var map = batchOption.map;
                var sizeMultiplier = batchOption.sizeMultiplier;

                if (map == null)
                {
                    Console.WriteLine("\nWARNING:\nMap is empty!\n[Object] Map in the index number " + bNumber + " is not defined.");
                    Thread.Sleep(3000);
                }
                else if (map.Length == 0)
                {
                    Console.WriteLine("\nWARNING:\nMap is empty!\n[Object] Map in the index number " + bNumber + " has no string arrays.");
                    Thread.Sleep(3000);
                }

                int xLength = map[0].Length;
                int yLength = map.Length;

                if (options.console.setup)
                    Console.WriteLine("Updating info...\n\nImage size set to " + (sizeMultiplier * 2 + sizeMultiplier * xLength) + "px width and " + (sizeMultiplier * 2 + sizeMultiplier * yLength) + "px height.\nBiome set to \"" + tiledata.biomes[batchOption.biome - 1].name + "\"\n");

                Bitmap b = new Bitmap(sizeMultiplier * 2 + sizeMultiplier * xLength, sizeMultiplier * 2 + sizeMultiplier * yLength);
                Graphics g = Graphics.FromImage(b);

                int currentY = 0;
                int currentX = 0;

                if (options.console.setup)
                    Console.WriteLine("Fetching tile colors...");

                string[] color1s = tiledata.biomes[batchOption.biome - 1].color1.Split(',');
                Color color1 = Color.FromArgb(int.Parse(color1s[0].Trim()), int.Parse(color1s[1].Trim()), int.Parse(color1s[2].Trim()));

                string[] color2s = tiledata.biomes[batchOption.biome - 1].color2.Split(',');
                Color color2 = Color.FromArgb(int.Parse(color2s[0].Trim()), int.Parse(color2s[1].Trim()), int.Parse(color2s[2].Trim()));

                if (options.console.setup)
                    Console.WriteLine("Coloring the tiles...");

                foreach (string row in map)
                {

                    foreach (char tile in row.ToCharArray())
                    {
                        if (batchOption.skipTiles.Contains(tile))
                        {
                            currentX++;
                            continue;
                        }

                        if (currentY % 2 == 0)
                        {
                            if (currentX % 2 == 0)
                                g.FillRectangle(new SolidBrush(color1), sizeMultiplier + sizeMultiplier * currentX, sizeMultiplier + sizeMultiplier * currentY, sizeMultiplier, sizeMultiplier);
                            else
                                g.FillRectangle(new SolidBrush(color2), sizeMultiplier + sizeMultiplier * currentX, sizeMultiplier + sizeMultiplier * currentY, sizeMultiplier, sizeMultiplier);
                        }
                        else
                        {
                            if (currentX % 2 == 0)
                                g.FillRectangle(new SolidBrush(color2), sizeMultiplier + sizeMultiplier * currentX, sizeMultiplier + sizeMultiplier * currentY, sizeMultiplier, sizeMultiplier);
                            else
                                g.FillRectangle(new SolidBrush(color1), sizeMultiplier + sizeMultiplier * currentX, sizeMultiplier + sizeMultiplier * currentY, sizeMultiplier, sizeMultiplier);
                        }

                        currentX++;

                    }

                    currentX = 0;
                    currentY++;

                }

                currentY = 0;
                currentX = 0;

                List<OrderedTile> orderedTiles = new List<OrderedTile>();

                Console.WriteLine("Drawing map no" + bNumber + "...");

                foreach (string row in map)
                {
                    List<OrderedTile> orderedHorTiles = new List<OrderedTile>();

                    foreach (char tTile in row.ToCharArray())
                    {
                        if (batchOption.skipTiles.Contains(tTile))
                        {
                            if (options.console.tileDraw)
                                Console.WriteLine(" s    [" + tTile + "] < y: " + SpaceFiller(currentY, yLength.ToString().ToCharArray().Length, ' ') + " / x: " + SpaceFiller(currentX, xLength.ToString().ToCharArray().Length, ' ') + " >  SKIPPED.");
                            currentX++;
                            continue;
                        }

                        var tile = tTile;

                        foreach (Options.Replace repTile in batchOption.replaceTiles)
                        {
                            if (tile == repTile.from)
                                tile = repTile.to;
                        }

                        string NeighborBinary = "";

                        foreach (Tiledata.Tile aTile in tiledata.tiles)
                        {
                            if (aTile.tileCode == tile)
                            {
                                foreach (Tiledata.TileDefault tileDefault in tiledata.biomes[batchOption.biome - 1].defaults)
                                {
                                    if (tileDefault.tile == aTile.tileName)
                                    {
                                        if (aTile.tileLinks != null)
                                        {
                                            if (currentY != 0 && currentX != 0) { if (map[currentY - 1].ToCharArray()[currentX - 1] == aTile.tileCode) { NeighborBinary = "1"; } else NeighborBinary = "0"; } else NeighborBinary = "0";
                                            if (currentY != 0) { if (map[currentY - 1].ToCharArray()[currentX] == aTile.tileCode) { NeighborBinary = NeighborBinary + "1"; } else NeighborBinary = NeighborBinary + "0"; } else NeighborBinary = NeighborBinary + "0";
                                            if (currentY != 0 && currentX != xLength - 1) { if (map[currentY - 1].ToCharArray()[currentX + 1] == aTile.tileCode) { NeighborBinary = NeighborBinary + "1"; } else NeighborBinary = NeighborBinary + "0"; } else NeighborBinary = NeighborBinary + "0";
                                            if (currentX != 0) { if (map[currentY].ToCharArray()[currentX - 1] == aTile.tileCode) { NeighborBinary = NeighborBinary + "1"; } else NeighborBinary = NeighborBinary + "0"; } else NeighborBinary = NeighborBinary + "0";
                                            if (currentX != xLength - 1) { if (map[currentY].ToCharArray()[currentX + 1] == aTile.tileCode) { NeighborBinary = NeighborBinary + "1"; } else NeighborBinary = NeighborBinary + "0"; } else NeighborBinary = NeighborBinary + "0";
                                            if (currentY != yLength - 1 && currentX != 0) { if (map[currentY + 1].ToCharArray()[currentX - 1] == aTile.tileCode) { NeighborBinary = NeighborBinary + "1"; } else NeighborBinary = NeighborBinary + "0"; } else NeighborBinary = NeighborBinary + "0";
                                            if (currentY != yLength - 1) { if (map[currentY + 1].ToCharArray()[currentX] == aTile.tileCode) { NeighborBinary = NeighborBinary + "1"; } else NeighborBinary = NeighborBinary + "0"; } else NeighborBinary = NeighborBinary + "0";
                                            if (currentY != yLength - 1 && currentX != xLength - 1) { if (map[currentY + 1].ToCharArray()[currentX + 1] == aTile.tileCode) { NeighborBinary = NeighborBinary + "1"; } else NeighborBinary = NeighborBinary + "0"; } else NeighborBinary = NeighborBinary + "0";

                                            List<Tiledata.TileLinkRule> accurateRules = new List<Tiledata.TileLinkRule>();

                                            var nbca = NeighborBinary.ToCharArray();
                                            if (aTile.tileLinks.rules.Length != 0)
                                            {
                                                foreach (var rule in aTile.tileLinks.rules)
                                                {
                                                    int accuracy = 0;
                                                    for (int x = 0; x < 8; x++)
                                                    {
                                                        if (rule.condition.Contains('!'))
                                                        {
                                                            if (rule.condition.Replace("!", "").ToCharArray()[x] == '*')
                                                                accuracy++;
                                                            else if (rule.condition.Replace("!", "").ToCharArray()[x] != nbca[x])
                                                                accuracy++;
                                                        }
                                                        else
                                                        {
                                                            if (rule.condition.ToCharArray()[x] == '*')
                                                                accuracy++;
                                                            else if (rule.condition.ToCharArray()[x] == nbca[x])
                                                                accuracy++;
                                                        }
                                                    }

                                                    if (accuracy == 8)
                                                        accurateRules.Add(rule);
                                                }
                                            }

                                            var defaultType = aTile.tileTypes[aTile.tileLinks.defaults.tileType - 1];
                                            foreach (Tiledata.TileLinkRule aRule in accurateRules)
                                            {
                                                if (aRule.changeBinary != null)
                                                    for (int y = 0; y < aRule.changeBinary.Length; y++)
                                                    {
                                                        nbca[int.Parse(aRule.changeBinary[y].Split('a')[1]) - 1] = aRule.changeBinary[y].Split('a')[0].ToCharArray()[0];
                                                    }
                                                if (aRule.changeTileType != null)
                                                    defaultType = aTile.tileTypes[aRule.changeTileType.GetValueOrDefault() - 1];
                                            }

                                            var defaultAsset = defaultType.asset;
                                            if (defaultAsset.Contains("!"))
                                                defaultAsset = defaultAsset.Split('!')[0];

                                            var fullBinaryFinal = string.Join("", nbca);

                                            if (defaultAsset.Contains("?binary?"))
                                                defaultAsset = defaultAsset.Replace("?binary?", fullBinaryFinal);

                                            string fols = "";
                                            if (aTile.tileLinks.assetFolder != null)
                                                fols = aTile.tileLinks.assetFolder + "\\";
                                            var assetst = fullBinaryFinal + ".svg";

                                            if (defaultType.order != null)
                                            {
                                                if (options.console.tileDraw)
                                                    Console.WriteLine("  o   [" + tTile + "] < y: " + SpaceFiller(currentY, yLength.ToString().ToCharArray().Length, ' ') + " / x: " + SpaceFiller(currentX, xLength.ToString().ToCharArray().Length, ' ') + " >  DELAYED FOR ORDERING.");
                                                orderedTiles.Add(new OrderedTile()
                                                {
                                                    tileType = defaultType,
                                                    xPosition = currentX,
                                                    yPosition = currentY,
                                                    tileCode = aTile.tileCode,
                                                    tileName = aTile.tileName
                                                });
                                                continue;
                                            }
                                            if (defaultType.orderHor != null)
                                            {
                                                if (options.console.tileDraw)
                                                    Console.WriteLine("  oh  [" + tTile + "] < y: " + SpaceFiller(currentY, yLength.ToString().ToCharArray().Length, ' ') + " / x: " + SpaceFiller(currentX, xLength.ToString().ToCharArray().Length, ' ') + " >  DELAYED FOR HORIZONTAL ORDERING.");
                                                orderedHorTiles.Add(new OrderedTile()
                                                {
                                                    tileType = defaultType,
                                                    xPosition = currentX,
                                                    yPosition = currentY,
                                                    tileCode = aTile.tileCode,
                                                    tileName = aTile.tileName
                                                });
                                                continue;
                                            }

                                            var li = SvgDocument.Open("assets\\tiles\\" + options.preset + "\\" + fols + defaultAsset);
                                            var liw = (int)Math.Round(li.Width * sizeMultiplier);
                                            var lih = (int)Math.Round(li.Height * sizeMultiplier);
                                            var lihm = (int)Math.Round((double)defaultType.tileParts.top * sizeMultiplier / 1000);
                                            var liwm = (int)Math.Round((double)defaultType.tileParts.left * sizeMultiplier / 1000);

                                            if (options.console.tileDraw)
                                                Console.WriteLine("    d [" + tTile + "] < y: " + SpaceFiller(currentY, yLength.ToString().ToCharArray().Length, ' ') + " / x: " + SpaceFiller(currentX, xLength.ToString().ToCharArray().Length, ' ') + " >  DRAWN AS \"" + aTile.tileName.ToUpper() + "\".");

                                            g.DrawImage(li.Draw(liw, lih), sizeMultiplier + sizeMultiplier * currentX - liwm, sizeMultiplier + sizeMultiplier * currentY - lihm);
                                            continue;
                                        }

                                        if (aTile.tileTypes[tileDefault.type - 1].order != null)
                                        {
                                            if (options.console.tileDraw)
                                                Console.WriteLine("  o   [" + tTile + "] < y: " + SpaceFiller(currentY, yLength.ToString().ToCharArray().Length, ' ') + " / x: " + SpaceFiller(currentX, xLength.ToString().ToCharArray().Length, ' ') + " >  DELAYED FOR ORDERING.");
                                            orderedTiles.Add(new OrderedTile()
                                            {
                                                tileType = aTile.tileTypes[tileDefault.type - 1],
                                                xPosition = currentX,
                                                yPosition = currentY,
                                                tileCode = aTile.tileCode,
                                                tileName = aTile.tileName
                                            });
                                            continue;
                                        }
                                        if (aTile.tileTypes[tileDefault.type - 1].orderHor != null)
                                        {
                                            if (options.console.tileDraw)
                                                Console.WriteLine("  oh  [" + tTile + "] < y: " + SpaceFiller(currentY, yLength.ToString().ToCharArray().Length, ' ') + " / x: " + SpaceFiller(currentX, xLength.ToString().ToCharArray().Length, ' ') + " >  DELAYED FOR HORIZONTAL ORDERING.");
                                            orderedHorTiles.Add(new OrderedTile()
                                            {
                                                tileType = aTile.tileTypes[tileDefault.type - 1],
                                                xPosition = currentX,
                                                yPosition = currentY,
                                                tileCode = aTile.tileCode,
                                                tileName = aTile.tileName
                                            });
                                            continue;
                                        }

                                        var i = SvgDocument.Open("assets\\tiles\\" + options.preset + "\\" + aTile.tileTypes[tileDefault.type - 1].asset);
                                        var iw = (int)Math.Round(i.Width * sizeMultiplier);
                                        var ih = (int)Math.Round(i.Height * sizeMultiplier);
                                        var ihm = (int)Math.Round((double)aTile.tileTypes[tileDefault.type - 1].tileParts.top * sizeMultiplier / 1000);
                                        var iwm = (int)Math.Round((double)aTile.tileTypes[tileDefault.type - 1].tileParts.left * sizeMultiplier / 1000);

                                        if (options.console.tileDraw)
                                            Console.WriteLine("    d [" + tTile + "] < y: " + SpaceFiller(currentY, yLength.ToString().ToCharArray().Length, ' ') + " / x: " + SpaceFiller(currentX, xLength.ToString().ToCharArray().Length, ' ') + " >  DRAWN AS \"" + aTile.tileName.ToUpper() + "\".");

                                        g.DrawImage(i.Draw(iw, ih), sizeMultiplier + sizeMultiplier * currentX - iwm, sizeMultiplier + sizeMultiplier * currentY - ihm);
                                        continue;
                                    }
                                }
                            }
                        }

                        currentX++;

                    }

                    int currentHorOrdered = 0;
                    ++currentHorOrdered;
                    foreach (var pTile in orderedHorTiles)
                    {
                        if (pTile == null)
                            continue;
                        if (pTile.tileType.orderHor.GetValueOrDefault() != currentHorOrdered)
                            continue;
                        var i = SvgDocument.Open("assets\\tiles\\" + options.preset + "\\" + pTile.tileType.asset);
                        var iw = (int)Math.Round(i.Width * sizeMultiplier);
                        var ih = (int)Math.Round(i.Height * sizeMultiplier);
                        var ihm = (int)Math.Round((double)pTile.tileType.tileParts.top * sizeMultiplier / 1000);
                        var iwm = (int)Math.Round((double)pTile.tileType.tileParts.left * sizeMultiplier / 1000);

                        if (options.console.orderedHorTileDraw)
                            Console.WriteLine("  ohd [" + pTile.tileCode + "] < y: " + SpaceFiller(pTile.yPosition, yLength.ToString().ToCharArray().Length, ' ') + " / x: " + SpaceFiller(pTile.xPosition, xLength.ToString().ToCharArray().Length, ' ') + " >  DRAWN HORIZONTALLY ORDERED TILE AS \"" + pTile.tileName.ToUpper() + "\".");

                        g.DrawImage(i.Draw(iw, ih), sizeMultiplier + sizeMultiplier * pTile.xPosition - iwm, sizeMultiplier + sizeMultiplier * pTile.yPosition - ihm);
                    }

                    currentX = 0;
                    currentY++;

                }

                int currentOrdered = 0;
                ++currentOrdered;
                foreach (var pTile in orderedTiles)
                {
                    if (pTile == null)
                        continue;
                    if (pTile.tileType.order.GetValueOrDefault() != currentOrdered)
                        continue;
                    var i = SvgDocument.Open("assets\\tiles\\" + options.preset + "\\" + pTile.tileType.asset);
                    var iw = (int)Math.Round(i.Width * sizeMultiplier);
                    var ih = (int)Math.Round(i.Height * sizeMultiplier);
                    var ihm = (int)Math.Round((double)pTile.tileType.tileParts.top * sizeMultiplier / 1000);
                    var iwm = (int)Math.Round((double)pTile.tileType.tileParts.left * sizeMultiplier / 1000);

                    if (options.console.orderedTileDraw)
                        Console.WriteLine("  o d [" + pTile.tileCode + "] < y: " + SpaceFiller(pTile.yPosition, yLength.ToString().ToCharArray().Length, ' ') + " / x: " + SpaceFiller(pTile.xPosition, xLength.ToString().ToCharArray().Length, ' ') + " >  DRAWN ORDERED TILE AS \"" + pTile.tileName.ToUpper() + "\".");

                    g.DrawImage(i.Draw(iw, ih), sizeMultiplier + sizeMultiplier * pTile.xPosition - iwm, sizeMultiplier + sizeMultiplier * pTile.yPosition - ihm);
                }

                string exportName = options.exportFileName;

                if (exportName.Contains("?number?"))
                {
                    string bNumberText = SpaceFiller(bNumber, 4, '0');

                    exportName = exportName.Replace("?number?", bNumberText);
                }

                if (options.exportFolderName != null)
                {
                    if (options.exportFolderName.Trim() != "")
                    {
                        if (!Directory.Exists(options.exportFolderName))
                            Directory.CreateDirectory(options.exportFolderName);
                        b.Save(options.exportFolderName + "\\" + exportName, ImageFormat.Png);
                        if (options.console.aal)
                            Console.WriteLine("[ AAL ] WRITE >> " + options.exportFolderName + "\\" + exportName);
                        if (options.console.saveLocation)
                            Console.WriteLine("\nImage saved to " + Environment.CurrentDirectory + options.exportFolderName + "\\" + exportName + ".");
                    }
                    else
                    {
                        b.Save(exportName, ImageFormat.Png);
                        if (options.console.aal)
                            Console.WriteLine("[ AAL ] WRITE >> " + exportName);
                        Console.WriteLine("\nImage saved to " + Environment.CurrentDirectory + exportName + ".");
                    }
                }
                else
                {
                    b.Save(exportName, ImageFormat.Png);
                    if (options.console.aal)
                        Console.WriteLine("[ AAL ] WRITE >> " + exportName);
                    Console.WriteLine("\nImage saved to " + Environment.CurrentDirectory + exportName + ".");
                }

            }

            Console.WriteLine("\nFinished.");

            Console.ReadKey();

        }

        public static string SpaceFiller(string text, int minAmountOfChar, char filler)
        {
            var c = text.ToCharArray();
            string t = "";
            
            if (c.Length < minAmountOfChar)
            {
                for (int x = 0; x < minAmountOfChar - c.Length; x++)
                    t = filler + t;
                return t + text;
            }
            return text;
        }

        public static string SpaceFiller(int number, int minAmountOfChar, char filler)
        {
            var c = number.ToString().ToCharArray();
            string t = "";

            if (c.Length < minAmountOfChar)
            {
                for (int x = 0; x < minAmountOfChar - c.Length; x++)
                    t = filler + t;
                return t + number;
            }
            return number.ToString();
        }

    }

}