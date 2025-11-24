// CommandParserUtility.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CAP_ChatInteractive.Utilities
{
    public class ParsedCommand
    {
        public string ItemName { get; set; } = "";
        public string Quality { get; set; } = "random";
        public string Material { get; set; } = "random";
        public string Side { get; set; } = null;
        public int Quantity { get; set; } = 1;
        public string Error { get; set; } = null;

        public bool HasError => Error != null;
    }

    public static class CommandParserUtility
    {
        private static HashSet<string> _materialKeywords = null;

        public static ParsedCommand ParseCommandArguments(string[] args, bool allowQuality = true, bool allowMaterial = true, bool allowSide = false, bool allowQuantity = true)
        {
            var result = new ParsedCommand();

            if (args.Length == 0)
            {
                result.Error = "Usage: !command<buy use equip wear surgery> <item> [quality] [material] [side] [quantity]";
                return result;
            }

            // Step 1: Clean and normalize arguments
            var cleanedArgs = CleanArguments(args);

            // Step 2: Parse from END to START (reverse order) but with multi-word material support
            var remainingArgs = new List<string>(cleanedArgs);

            // Parse quantity FIRST (last argument if it's a number)
            if (allowQuantity && remainingArgs.Count > 0 && int.TryParse(remainingArgs[remainingArgs.Count - 1], out int quantity))
            {
                result.Quantity = quantity;
                remainingArgs.RemoveAt(remainingArgs.Count - 1);
            }

            // Parse side (if allowed and available)
            if (allowSide && remainingArgs.Count > 0 && IsSideKeyword(remainingArgs[remainingArgs.Count - 1]))
            {
                result.Side = remainingArgs[remainingArgs.Count - 1];
                remainingArgs.RemoveAt(remainingArgs.Count - 1);
            }

            // NEW: Check for multi-word materials before single-word materials
            string foundMaterial = null;
            if (allowMaterial && remainingArgs.Count > 0)
            {
                // Try 3-word material first, then 2-word, then 1-word
                for (int wordCount = 3; wordCount >= 1; wordCount--)
                {
                    if (remainingArgs.Count >= wordCount)
                    {
                        var materialWords = remainingArgs.Skip(remainingArgs.Count - wordCount).Take(wordCount).ToArray();
                        string potentialMaterial = string.Join(" ", materialWords);

                        if (IsMaterialKeyword(potentialMaterial))
                        {
                            foundMaterial = potentialMaterial;
                            // Remove the material words from remaining args
                            remainingArgs.RemoveRange(remainingArgs.Count - wordCount, wordCount);
                            break;
                        }
                    }
                }
            }

            // Parse quality (if allowed and available)
            if (allowQuality && remainingArgs.Count > 0 && IsQualityKeyword(remainingArgs[remainingArgs.Count - 1]))
            {
                result.Quality = remainingArgs[remainingArgs.Count - 1];
                remainingArgs.RemoveAt(remainingArgs.Count - 1);
            }

            // Set the material we found (if any)
            result.Material = foundMaterial ?? "random";

            // CRITICAL FIX: If no item name remains but we found a material, use the material as the item name
            if (remainingArgs.Count == 0 && foundMaterial != null)
            {
                result.ItemName = foundMaterial;
                result.Material = "random"; // Reset material since it was actually the item
            }
            else if (remainingArgs.Count > 0)
            {
                result.ItemName = string.Join(" ", remainingArgs).Trim();
            }
            else
            {
                result.Error = "No item name specified.";
                return result;
            }

            // Validate item name isn't empty after cleanup
            if (string.IsNullOrWhiteSpace(result.ItemName))
            {
                result.Error = "Invalid item name after parsing arguments.";
                return result;
            }

            Logger.Debug($"Parsed - Item: '{result.ItemName}', Quality: '{result.Quality}', Material: '{result.Material}', Side: '{result.Side}', Quantity: {result.Quantity}");

            return result;
        }
        private static string[] CleanArguments(string[] args)
        {
            var cleaned = new List<string>();

            foreach (string arg in args)
            {
                // Remove/replace problematic characters with spaces
                string cleanArg = arg.Replace("[", " ")
                                   .Replace("]", " ")
                                 //  .Replace("(", " ")
                                 //  .Replace(")", " ")
                                   .Replace(",", " ")
                                   .Replace(".", " ")
                                   .Replace(";", " ")
                                   .Trim();

                // Split if cleaning created multiple words (e.g., "axe[awful" -> "axe awful")
                var words = cleanArg.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                cleaned.AddRange(words);
            }

            return cleaned.ToArray();
        }

        private static bool IsQualityKeyword(string arg)
        {
            return arg.ToLower() switch
            {
                "awful" or "poor" or "normal" or "good" or "excellent" or "masterwork" or "legendary" => true,
                _ => false
            };
        }

        public static bool IsMaterialKeyword(string arg)
        {
            InitializeMaterialKeywords();
            return _materialKeywords.Contains(arg);
        }

        private static bool IsSideKeyword(string arg)
        {
            return arg.ToLower() switch
            {
                "left" or "right" or "l" or "r" => true,
                _ => false
            };
        }

        private static void InitializeMaterialKeywords()
        {
            if (_materialKeywords != null) return;

            _materialKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var allStuffDefs = DefDatabase<ThingDef>.AllDefs.Where(def => def.IsStuff);
                foreach (var stuffDef in allStuffDefs)
                {
                    // Add def name (with underscores as spaces)
                    string defNameWithSpaces = stuffDef.defName.Replace("_", " ");
                    _materialKeywords.Add(defNameWithSpaces);

                    // Add def name (original with underscores)
                    _materialKeywords.Add(stuffDef.defName);

                    // Add label (this is the display name with spaces)
                    if (!string.IsNullOrEmpty(stuffDef.label))
                    {
                        _materialKeywords.Add(stuffDef.label);

                        // Also add label without spaces for backward compatibility
                        _materialKeywords.Add(stuffDef.label.Replace(" ", ""));
                    }
                }

                Logger.Debug($"Initialized material keywords with {_materialKeywords.Count} entries");

                // Log some examples for debugging
                var blockMaterials = _materialKeywords.Where(k => k.Contains("block")).Take(5).ToList();
                Logger.Debug($"Example block materials: {string.Join(", ", blockMaterials)}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error initializing material keywords: {ex}");
                // Fallback to common materials including multi-word ones
                _materialKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "wood", "steel", "plasteel", "cloth", "leather", "synthread", "hyperweave",
            "gold", "silver", "uranium", "jade", "component", "components",
            "marble blocks", "granite blocks", "limestone blocks", "sandstone blocks",
            "slate blocks", "steel blocks", "plasteel blocks", "wood logs"
        };
            }
        }
    }
}