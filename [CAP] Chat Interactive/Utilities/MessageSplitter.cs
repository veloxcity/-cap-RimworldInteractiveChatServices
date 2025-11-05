// MessageSplitter.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
// Utility class to split long messages for different chat platforms
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAP_ChatInteractive.Utilities
{
    public static class MessageSplitter
    {
        private const int TWITCH_MAX_LENGTH = 500;
        private const int YOUTUBE_MAX_LENGTH = 200;

        public static List<string> SplitMessage(string message, string platform, string username = null)
        {
            int maxLength = GetPlatformMaxLength(platform);

            if (message.Length <= maxLength)
                return new List<string> { message };

            return SplitLongMessage(message, maxLength, username, platform);
        }

        private static int GetPlatformMaxLength(string platform)
        {
            return platform.ToLowerInvariant() switch
            {
                "youtube" => YOUTUBE_MAX_LENGTH,
                "twitch" => TWITCH_MAX_LENGTH,
                _ => YOUTUBE_MAX_LENGTH
            };
        }

        private static List<string> SplitLongMessage(string message, int maxLength, string username, string platform)
        {
            return platform.ToLowerInvariant() == "youtube"
                ? SplitForYouTube(message, maxLength, username)
                : SplitForTwitch(message, maxLength, username);
        }

        private static List<string> SplitForTwitch(string message, int maxLength, string username)
        {
            var parts = new List<string>();
            string prefix = username != null ? $"@{username} " : "";
            int prefixLength = prefix.Length;
            int effectiveMax = maxLength - prefixLength;

            if (message.Contains("Available weather:") || message.Contains("Available commands:"))
            {
                return SplitListMessage(message, effectiveMax, prefix);
            }

            // Simply split by max length without sentence logic
            return SplitByMaxLengthOnly(message, effectiveMax, prefix);
        }

        private static List<string> SplitForYouTube(string message, int maxLength, string username)
        {
            var parts = new List<string>();
            string prefix = username != null ? $"@{username} " : "";
            int prefixLength = prefix.Length;
            int effectiveMax = maxLength - prefixLength;

            if (message.Contains("Available weather:") || message.Contains("Available commands:"))
            {
                return SplitListMessage(message, effectiveMax, prefix);
            }

            // Simply split by max length without sentence logic
            return SplitByMaxLengthOnly(message, effectiveMax, prefix);
        }

        private static List<string> SplitByMaxLengthOnly(string message, int maxLength, string prefix)
        {
            var parts = new List<string>();

            // If the entire message fits, just return it
            if (message.Length <= maxLength)
            {
                parts.Add(prefix + message);
                return parts;
            }

            var words = message.Split(' ');
            var currentPart = new StringBuilder();

            foreach (var word in words)
            {
                // Check if adding this word would exceed max length
                if (currentPart.Length + word.Length + 1 > maxLength)
                {
                    // If current part has content, add it to parts
                    if (currentPart.Length > 0)
                    {
                        parts.Add(prefix + currentPart.ToString().Trim());
                        currentPart.Clear();
                    }

                    // If a single word is too long, split it
                    if (word.Length > maxLength)
                    {
                        var wordChunks = SplitLongWord(word, maxLength);
                        parts.AddRange(wordChunks.Select(chunk => prefix + chunk));
                        continue;
                    }
                }

                // Add the word to current part
                if (currentPart.Length > 0)
                    currentPart.Append(" ");
                currentPart.Append(word);
            }

            // Add any remaining content
            if (currentPart.Length > 0)
            {
                parts.Add(prefix + currentPart.ToString().Trim());
            }

            return AddPagination(parts, prefix);
        }

        private static List<string> SplitLongWord(string word, int maxLength)
        {
            var chunks = new List<string>();
            for (int i = 0; i < word.Length; i += maxLength)
            {
                chunks.Add(word.Substring(i, Math.Min(maxLength, word.Length - i)));
            }
            return chunks;
        }

        // Your extraction methods (they're correct!)
        private static List<string> ExtractWeatherItems(string message)
        {
            var itemsPart = message.Substring("Available weather:".Length).Trim();
            var items = itemsPart.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(i => i.Trim())
                                 .ToList();
            return items;
        }

        private static List<string> ExtractCommandItems(string message)
        {
            var itemsPart = message.Substring("Available commands:".Length).Trim();
            var items = itemsPart.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(i => i.Trim())
                                 .ToList();
            return items;
        }

        private static List<string> SplitListMessage(string message, int maxLength, string prefix)
        {
            if (message.StartsWith("Available weather:"))
            {
                var items = ExtractWeatherItems(message);
                return BuildListPages(items, "Available weather", maxLength, prefix);
            }
            else if (message.StartsWith("Available commands:"))
            {
                var items = ExtractCommandItems(message);
                return BuildListPages(items, "Available commands", maxLength, prefix);
            }

            return new List<string> { prefix + message };
        }

        private static List<string> BuildListPages(List<string> items, string title, int maxLength, string prefix)
        {
            var pages = new List<string>();
            var currentPage = new StringBuilder();
            currentPage.Append(title + ": ");

            foreach (var item in items)
            {
                if (currentPage.Length + item.Length + 2 > maxLength)
                {
                    pages.Add(prefix + currentPage.ToString().TrimEnd(',', ' '));
                    currentPage.Clear();
                    currentPage.Append(title + " (cont.): ");
                }

                currentPage.Append(item + ", ");
            }

            if (currentPage.Length > title.Length + 2)
            {
                pages.Add(prefix + currentPage.ToString().TrimEnd(',', ' '));
            }

            return AddPagination(pages, prefix);
        }

        private static List<string> AddPagination(List<string> parts, string prefix)
        {
            if (parts.Count > 1)
            {
                for (int i = 0; i < parts.Count; i++)
                {
                    var cleanPart = parts[i];
                    if (!string.IsNullOrEmpty(prefix) && cleanPart.StartsWith(prefix))
                    {
                        cleanPart = cleanPart.Substring(prefix.Length);
                    }
                    parts[i] = $"{prefix}{cleanPart} ({i + 1}/{parts.Count})";

                    // Check if adding pagination made it too long
                    if (parts[i].Length > GetPlatformMaxLength("twitch")) // Use appropriate platform
                    {
                        // If too long, truncate and add pagination
                        int maxWithoutPagination = GetPlatformMaxLength("twitch") - $" ({i + 1}/{parts.Count})".Length;
                        if (cleanPart.Length > maxWithoutPagination)
                        {
                            cleanPart = cleanPart.Substring(0, maxWithoutPagination - 3) + "...";
                        }
                        parts[i] = $"{prefix}{cleanPart} ({i + 1}/{parts.Count})";
                    }
                }
            }
            return parts;
        }
    }
}