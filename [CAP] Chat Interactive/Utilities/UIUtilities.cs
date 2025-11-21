// UIUtilities.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
// General UI utility methods for the CAP Chat Interactive mod

using UnityEngine;
using Verse;

namespace CAP_ChatInteractive
{
    public static class UIUtilities
    {
        /// <summary>
        /// Truncates text to fit within a specified width, adding ".." if necessary
        /// </summary>
        /// <param name="text">The text to truncate</param>
        /// <param name="maxWidth">The maximum width in pixels</param>
        /// <returns>Truncated text with ".." if it was too long</returns>
        public static string TruncateTextToWidth(string text, float maxWidth)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Account for button padding (approximately 2 characters)
            float paddingWidth = Text.CalcSize("M").x * 2f;
            float availableWidth = maxWidth - paddingWidth;

            // If it already fits, return as is
            if (Text.CalcSize(text).x <= availableWidth)
                return text;

            // Simple linear approach - remove characters until it fits
            string truncated = text;
            for (int i = text.Length - 1; i > 0; i--)
            {
                truncated = text.Substring(0, i) + "..";
                if (Text.CalcSize(truncated).x <= availableWidth)
                    break;
            }

            return truncated;
        }

        /// <summary>
        /// Truncates text to fit within a specified width, with custom ellipsis
        /// </summary>
        /// <param name="text">The text to truncate</param>
        /// <param name="maxWidth">The maximum width in pixels</param>
        /// <param name="ellipsis">The ellipsis string to use (default "..")</param>
        /// <returns>Truncated text with ellipsis if it was too long</returns>
        public static string TruncateTextToWidth(string text, float maxWidth, string ellipsis)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            float paddingWidth = Text.CalcSize("M").x * 2f;
            float availableWidth = maxWidth - paddingWidth;

            if (Text.CalcSize(text).x <= availableWidth)
                return text;

            string truncated = text;
            for (int i = text.Length - 1; i > 0; i--)
            {
                truncated = text.Substring(0, i) + ellipsis;
                if (Text.CalcSize(truncated).x <= availableWidth)
                    break;
            }

            return truncated;
        }

        /// <summary>
        /// Calculates if text will fit within the specified width
        /// </summary>
        /// <param name="text">The text to check</param>
        /// <param name="maxWidth">The maximum width in pixels</param>
        /// <returns>True if the text fits, false otherwise</returns>
        public static bool TextFitsWidth(string text, float maxWidth)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            float paddingWidth = Text.CalcSize("M").x * 2f;
            float availableWidth = maxWidth - paddingWidth;
            return Text.CalcSize(text).x <= availableWidth;
        }

        /// <summary>
        /// More efficient binary search approach for truncation
        /// </summary>
        public static string TruncateTextToWidthEfficient(string text, float maxWidth, string ellipsis = "..")
        {
            if (string.IsNullOrEmpty(text) || TextFitsWidth(text, maxWidth))
                return text;

            float paddingWidth = Text.CalcSize("M").x * 2f;
            float availableWidth = maxWidth - paddingWidth;

            int min = 1;
            int max = text.Length;
            string result = text + ellipsis;

            while (min <= max)
            {
                int mid = (min + max) / 2;
                string test = text.Substring(0, mid) + ellipsis;

                if (Text.CalcSize(test).x <= availableWidth)
                {
                    result = test;
                    min = mid + 1;
                }
                else
                {
                    max = mid - 1;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the maximum characters that fit in a given width
        /// </summary>
        public static int GetMaxCharacters(string text, float maxWidth, string ellipsis = "..")
        {
            if (string.IsNullOrEmpty(text)) return 0;

            float availableWidth = maxWidth - Text.CalcSize("M").x * 2f;
            float ellipsisWidth = Text.CalcSize(ellipsis).x;

            for (int i = text.Length; i > 0; i--)
            {
                float width = Text.CalcSize(text.Substring(0, i)).x + ellipsisWidth;
                if (width <= availableWidth)
                    return i;
            }

            return 1;
        }

        // Add this method to UIUtilities.cs
        /// <summary>
        /// Checks if text would be truncated to fit within specified width
        /// </summary>
        /// <param name="text">The text to check</param>
        /// <param name="maxWidth">The maximum width in pixels</param>
        /// <returns>True if the text would be truncated, false otherwise</returns>
        public static bool WouldTruncate(string text, float maxWidth)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            float paddingWidth = Text.CalcSize("M").x * 2f;
            float availableWidth = maxWidth - paddingWidth;
            return Text.CalcSize(text).x > availableWidth;
        }

        // Add this method to UIUtilities.cs
        /// <summary>
        /// Draws a button with automatic truncation and tooltip if truncated
        /// </summary>
        /// <param name="rect">The button rectangle</param>
        /// <param name="text">The button text</param>
        /// <param name="tooltip">Optional tooltip text (if null, shows full text when truncated)</param>
        /// <returns>True if button clicked</returns>
        public static bool ButtonWithTruncation(Rect rect, string text, string tooltip = null)
        {
            string truncatedText = TruncateTextToWidth(text, rect.width);
            bool result = Widgets.ButtonText(rect, truncatedText);

            // Show tooltip if text was truncated or custom tooltip provided
            if (Mouse.IsOver(rect))
            {
                if (!string.IsNullOrEmpty(tooltip))
                {
                    TooltipHandler.TipRegion(rect, tooltip);
                }
                else if (WouldTruncate(text, rect.width))
                {
                    TooltipHandler.TipRegion(rect, text);
                }
            }

            return result;
        }

        /// <summary>
        /// Draws a button with truncation and custom tooltip handling
        /// </summary>
        public static bool ButtonWithTruncation(Rect rect, string text, TipSignal tooltip)
        {
            string truncatedText = TruncateTextToWidth(text, rect.width);
            bool result = Widgets.ButtonText(rect, truncatedText);

            if (Mouse.IsOver(rect))
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rect"></param>
        /// <param name="value"></param>
        /// <param name="buffer"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public static void TextFieldNumericFlexible<T>(Rect rect, ref T value, ref string buffer, T min, T max) where T : struct
        {
            // Let user type freely without immediate clamping
            string newBuffer = Widgets.TextField(rect, buffer);

            // Only parse and validate when the input actually changes
            if (newBuffer != buffer)
            {
                buffer = newBuffer;

                // Try to parse the input
                if (float.TryParse(buffer, out float floatValue))
                {
                    // Convert to the appropriate type and clamp
                    if (typeof(T) == typeof(int))
                    {
                        int intValue = (int)floatValue;
                        intValue = Mathf.Clamp(intValue, (int)(object)min, (int)(object)max);
                        value = (T)(object)intValue;
                        buffer = intValue.ToString();
                    }
                    else if (typeof(T) == typeof(float))
                    {
                        floatValue = Mathf.Clamp(floatValue, (float)(object)min, (float)(object)max);
                        value = (T)(object)floatValue;
                        buffer = floatValue.ToString("0.##");
                    }
                }
                // If parsing fails but buffer is empty, set to min value
                else if (string.IsNullOrEmpty(buffer))
                {
                    value = min;
                    buffer = min.ToString();
                }
            }
        }

    }
}