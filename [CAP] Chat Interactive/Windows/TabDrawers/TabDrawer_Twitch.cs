// TabDrawer_Twitch.cs
// Copyright (c) Captolamia
// This file is part of CAP Chat Interactive.
// 
// CAP Chat Interactive is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// CAP Chat Interactive is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with CAP Chat Interactive. If not, see <https://www.gnu.org/licenses/>.
// Draws the Twitch settings tab in the mod settings window
using CAP_ChatInteractive;
using RimWorld;
using UnityEngine;
using Verse;
using ColorLibrary = CAP_ChatInteractive.ColorLibrary;

namespace _CAP__Chat_Interactive
{
    public static class TabDrawer_Twitch
    {
        private static Vector2 _scrollPosition = Vector2.zero;

        public static void Draw(Rect region)
        {
            var settings = CAPChatInteractiveMod.Instance.Settings.TwitchSettings;
            var view = new Rect(0f, 0f, region.width - 16f, 750f); // Increased height

            Widgets.BeginScrollView(region, ref _scrollPosition, view);
            var listing = new Listing_Standard();
            listing.Begin(view);

            // === Twitch Tab Header ===
            Text.Font = GameFont.Medium;
            GUI.color = ColorLibrary.HeaderAccent;
            listing.Label("RICS.Twitch.TwitchIntegrationHeader".Translate());
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            // Quick start guid
            listing.Gap(8f);

            string quickGuide =
                "<b>" + "RICS.Twitch.QuickGuide.Title".Translate() + "</b>\n" +
                "RICS.Twitch.QuickGuide.Step1".Translate() + "\n" +
                "RICS.Twitch.QuickGuide.Step2".Translate() + "\n" +
                "RICS.Twitch.QuickGuide.Step3".Translate();

            float textHeight = Text.CalcHeight(quickGuide, listing.ColumnWidth) + 8f;
            Rect quickGuideRect = listing.GetRect(textHeight);
            Widgets.Label(quickGuideRect, quickGuide);
            TooltipHandler.TipRegion(quickGuideRect, "RICS.Twitch.QuickGuide.Tooltip".Translate()
            );

            listing.Gap(12f);

            // === Enable/Disable Integration ===
            Rect enableRect = listing.GetRect(30f);
            Widgets.CheckboxLabeled(enableRect,
                "RICS.Twitch.EnableIntegrationLabel".Translate(),
                ref settings.Enabled);

            string quickGuideTooltip =
                "RICS.Twitch.EnableIntegrationTooltip1".Translate() +
                "RICS.Twitch.EnableIntegrationTooltip2".Translate() +
                "RICS.Twitch.EnableIntegrationTooltip3".Translate();
            TooltipHandler.TipRegion(enableRect, quickGuideTooltip );

            listing.Gap(16f);

            // === Channel Name Section ===
            Text.Font = GameFont.Medium;
            GUI.color = ColorLibrary.SubHeader;
            listing.Label("RICS.Twitch.ChannelInformationHeader".Translate());
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            listing.GapLine(6f);
            listing.Gap(4f);

            // Channel name with proper tooltip on the label
            Rect channelLabelRect = listing.GetRect(24f);
            // OLD: Widgets.Label(channelLabelRect, "Channel Name:");
            Widgets.Label(channelLabelRect,
                "RICS.Twitch.ChannelNameLabel".Translate());
            /* OLD:
            TooltipHandler.TipRegion(channelLabelRect,
                "<b>Your Twitch Channel Name</b>\n\n" +
                "This is the name that appears in your stream URL:\n" +
                "• https://twitch.tv/<color=orange>YOUR_CHANNEL_NAME</color>\n\n" +
                "<b>Examples:</b>\n" +
                "• If your URL is twitch.tv/superstreamer → enter 'superstreamer'\n" +
                "• Case insensitive - 'SuperStreamer' same as 'superstreamer'\n\n" +
                "🔍 <i>This is NOT your display name with capitals!</i>");
            */
            TooltipHandler.TipRegion(channelLabelRect,
                "<b>" + UIUtilities.Colorize("RICS.Twitch.ChannelNameTooltip.Title".Translate(),ColorLibrary.HeaderAccent) // orange
                + "</b>\n\n" +
                "RICS.Twitch.ChannelNameTooltip.Desc".Translate() + "\n\n" +
                UIUtilities.Colorize(
                    "RICS.Twitch.ChannelNameTooltip.UrlExample".Translate(),
                    ColorLibrary.SubHeader  // sky blue
                ) + "\n\n" +
                "<b>" + "RICS.Twitch.ChannelNameTooltip.ExamplesHeader".Translate() + "</b>\n" +
                "• " + "RICS.Twitch.ChannelNameTooltip.Example1".Translate() + "\n" +
                "• " + "RICS.Twitch.ChannelNameTooltip.CaseNote".Translate() + "\n\n" +
                UIUtilities.Colorize(
                    "RICS.Twitch.ChannelNameTooltip.Warning".Translate(),
                    ColorLibrary.Warning       // subtle orange/red
                )
            );

            Rect channelFieldRect = listing.GetRect(30f);
            settings.ChannelName = Widgets.TextField(channelFieldRect, settings.ChannelName);
            // OLD: TooltipHandler.TipRegion(channelFieldRect, "Enter your Twitch channel name here");
            TooltipHandler.TipRegion(channelFieldRect,
                "RICS.Twitch.ChannelNameFieldTooltip".Translate()
            );
            listing.Gap(12f);

            // Bot Account Section
            Text.Font = GameFont.Medium;
            // OLD: listing.Label("Bot Account (Optional)");
            listing.Label("RICS.Twitch.BotAccountHeader".Translate());
            Text.Font = GameFont.Small;
            listing.GapLine(6f);
            listing.Gap(4f);

            // Bot username with proper tooltip
            Rect botLabelRect = listing.GetRect(24f);
            // OLD: Widgets.Label(botLabelRect, "Bot Username:");
            Widgets.Label(botLabelRect,
                "RICS.Twitch.BotUsernameLabel".Translate());
            TooltipHandler.TipRegion(botLabelRect,
                UIUtilities.Colorize(
                    "RICS.Twitch.BotUsernameTooltip.Title".Translate(),
                    ColorLibrary.HeaderAccent      // orange for main title
                ) + "\n\n" +

                UIUtilities.Colorize(
                    "RICS.Twitch.BotUsernameTooltip.Recommended".Translate(),
                    ColorLibrary.Success           // green – positive emphasis
                ) + "\n" +
                UIUtilities.Colorize(
                    "RICS.Twitch.BotUsernameTooltip.Alternative".Translate(),
                    ColorLibrary.MutedText         // gray – secondary option
                ) + "\n\n" +

                UIUtilities.Colorize(
                    "RICS.Twitch.BotUsernameTooltip.WhyHeader".Translate(),
                    ColorLibrary.SubHeader         // sky blue for sub-header
                ) + "\n" +
                "• " + "RICS.Twitch.BotUsernameTooltip.Why1".Translate() + "\n" +
                "• " + "RICS.Twitch.BotUsernameTooltip.Why2".Translate() + "\n" +
                "• " + "RICS.Twitch.BotUsernameTooltip.Why3".Translate() + "\n" +
                "• " + "RICS.Twitch.BotUsernameTooltip.Why4".Translate() + "\n\n" +

                UIUtilities.Colorize(
                    "RICS.Twitch.BotUsernameTooltip.MainAccountHeader".Translate(),
                    ColorLibrary.SubHeader         // sky blue again for consistency
                ) + "\n" +
                "• " + "RICS.Twitch.BotUsernameTooltip.Main1".Translate() + "\n" +
                "• " + "RICS.Twitch.BotUsernameTooltip.Main2".Translate()
            );

            Rect botFieldRect = listing.GetRect(30f);
            settings.BotUsername = Widgets.TextField(botFieldRect, settings.BotUsername);
            // OLD: TooltipHandler.TipRegion(botFieldRect, "Enter bot account username (or your main account)");
            TooltipHandler.TipRegion(botFieldRect,
                "RICS.Twitch.BotUsernameFieldTooltip".Translate()
            );

            // Bot account status - fixed to not get cut off
            if (!string.IsNullOrEmpty(settings.BotUsername) &&
                !string.IsNullOrEmpty(settings.ChannelName))
            {
                Rect botStatusRect = listing.GetRect(20f);
                if (settings.BotUsername.ToLower() != settings.ChannelName.ToLower())
                {
                    GUI.color = Color.green;
                    // OLD: Widgets.Label(botStatusRect, "✓ Using separate bot account");
                    Widgets.Label(botStatusRect,
                        "RICS.Twitch.BotAccountStatus.Separate".Translate());
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = Color.yellow;
                    // OLD: Widgets.Label(botStatusRect, "⚠ Using main streamer account as bot");
                    Widgets.Label(botStatusRect,
                        "RICS.Twitch.BotAccountStatus.Main".Translate());
                    GUI.color = Color.white;
                }
            }

            listing.Gap(16f);

            // OAuth Token Section
            Text.Font = GameFont.Medium;
            // OLD: listing.Label("Authentication");
            listing.Label("RICS.Twitch.AuthenticationHeader".Translate());
            Text.Font = GameFont.Small;
            listing.GapLine(6f);
            listing.Gap(4f);

            // Access Token label with tooltip
            Rect tokenLabelRect = listing.GetRect(24f);
            // OLD: Widgets.Label(tokenLabelRect, "Access Token:");
            Widgets.Label(tokenLabelRect,
                "RICS.Twitch.AccessTokenLabel".Translate());
            /* OLD:
            TooltipHandler.TipRegion(tokenLabelRect,
                "<b>Twitch OAuth Token</b>\n\n" +
                "🔐 This is like a password for the bot account\n" +
                "• Get it from Twitch Token Generator (button below)\n" +
                "• Token starts with 'oauth:'\n" +
                "• Keep this secret - never share it!\n\n" +
                "<b>How it works:</b>\n" +
                "1. Click 'Get Twitch Access Token' below\n" +
                "2. Login with your bot account (or main account)\n" +
                "3. Select 'Bot Chat Token'\n" +
                "4. Copy the generated token\n" +
                "5. Paste it here or use the Paste button\n\n" +
                "🛡️ <i>This token only allows chat access, not stream control</i>");
            */
            TooltipHandler.TipRegion(tokenLabelRect,
                "RICS.Twitch.AccessTokenTooltip".Translate()
            );

            // Token display field - read-only, masked for security
            Rect tokenFieldRect = listing.GetRect(30f);

            // Determine display text
            /* OLD:
                        string tokenDisplay = string.IsNullOrEmpty(settings.AccessToken) ?
                "[Click Paste or Get Token below]" :
                "oauth:••••••••••••••••";
            Add to translation files:
            <!-- Token display field -->
                <RICS.Twitch.AccessTokenEmpty>[No token set - use buttons below]</RICS.Twitch.AccessTokenEmpty>
                <RICS.Twitch.AccessTokenMasked>oauth:••••••••••••••••</RICS.Twitch.AccessTokenMasked>

                <!-- Tooltip for the field -->
                <RICS.Twitch.AccessTokenFieldTooltip>Twitch OAuth token (hidden for security).\nSelect text to copy, or use buttons below to paste/set.</RICS.Twitch.AccessTokenFieldTooltip>

            */
            string tokenDisplay;
            if (string.IsNullOrEmpty(settings.AccessToken))
            {
                tokenDisplay = "RICS.Twitch.AccessTokenEmpty".Translate();  // e.g. "[Click Paste or Get Token below]"
            }
            else
            {
                tokenDisplay = "RICS.Twitch.AccessTokenMasked".Translate(); // e.g. "oauth:••••••••••••••••"
            }

            // Draw the read-only field
            Widgets.TextField(tokenFieldRect, tokenDisplay);  // ← Add readOnly: true for clarity (optional but good)

            // Tooltip with translatable text
            TooltipHandler.TipRegion(tokenFieldRect,
                "RICS.Twitch.AccessTokenFieldTooltip".Translate()  // e.g. "Twitch OAuth token (click buttons below to set)"
            );

            // Token action buttons
            Rect tokenButtonRect = listing.GetRect(35f);
            Rect pasteRect = new Rect(tokenButtonRect.x, tokenButtonRect.y, 140f, 30f);
            Rect getTokenRect = new Rect(pasteRect.xMax + 10f, tokenButtonRect.y, 160f, 30f);

            /*  Add to translation files:
            <RICS.Twitch.PasteTokenButton>Paste Token</RICS.Twitch.PasteTokenButton>
            <RICS.Twitch.PasteTokenTooltip>Paste OAuth token from clipboard (auto-adds 'oauth:' if missing)</RICS.Twitch.PasteTokenTooltip>

            <RICS.Twitch.AddedOAuthPrefix>Added 'oauth:' prefix to token</RICS.Twitch.AddedOAuthPrefix>
            <RICS.Twitch.TokenPastedSuccess>Twitch token pasted successfully!</RICS.Twitch.TokenPastedSuccess>
            <RICS.Twitch.ClipboardEmpty>Clipboard is empty!</RICS.Twitch.ClipboardEmpty>
            */

            if (Widgets.ButtonText(pasteRect, "RICS.Twitch.PasteTokenButton".Translate()))
            {
                string clipboardText = GUIUtility.systemCopyBuffer?.Trim();
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    // Auto-add "oauth:" prefix if missing
                    if (!clipboardText.StartsWith("oauth:") && !clipboardText.Contains(" "))
                    {
                        clipboardText = "oauth:" + clipboardText;
                        Messages.Message("RICS.Twitch.AddedOAuthPrefix".Translate(), MessageTypeDefOf.SilentInput);
                    }
                    settings.AccessToken = clipboardText;
                    Messages.Message("RICS.Twitch.TokenPastedSuccess".Translate(), MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Messages.Message("RICS.Twitch.ClipboardEmpty".Translate(), MessageTypeDefOf.NegativeEvent);
                }
            }
            TooltipHandler.TipRegion(pasteRect, "RICS.Twitch.PasteTokenTooltip".Translate());

            if (Widgets.ButtonText(getTokenRect, "RICS.Twitch.GetTokenButton".Translate()))
            {
                string message =
                    "<b>" + "RICS.Twitch.TokenGeneratorTitle".Translate() + "</b>\n\n" +
                    "🔐 <b>" + "RICS.Twitch.TokenGeneratorStepsHeader".Translate() + "</b>\n" +
                    "RICS.Twitch.TokenGeneratorStep1".Translate() + "\n" +
                    "RICS.Twitch.TokenGeneratorStep2".Translate() + "\n" +
                    "RICS.Twitch.TokenGeneratorStep3".Translate() + "\n" +
                    "RICS.Twitch.TokenGeneratorStep4".Translate() + "\n" +
                    "RICS.Twitch.TokenGeneratorStep5".Translate() + "\n\n" +
                    "🔒 <b>" + "RICS.Twitch.TokenGeneratorSecurityHeader".Translate() + "</b>\n" +
                    "RICS.Twitch.TokenGeneratorSecurityNote1".Translate() + "\n" +
                    "RICS.Twitch.TokenGeneratorSecurityNote2".Translate() + "\n" +
                    "RICS.Twitch.TokenGeneratorSecurityNote3".Translate() + "\n\n" +
                    "RICS.Twitch.TokenGeneratorConfirmation".Translate();  // The final question

                Find.WindowStack.Add(new Dialog_MessageBox(
                    message,
                    "RICS.Twitch.OpenBrowserButton".Translate(),  // "Open Browser"
                    () => Application.OpenURL("https://twitchtokengenerator.com/"),
                    "RICS.Twitch.CancelButton".Translate(),       // "Cancel"
                    null, null, true  // Make it dismissible with Esc, etc.
                ));
            }
            TooltipHandler.TipRegion(getTokenRect, "RICS.Twitch.GetTokenButtonTooltip".Translate());

            // Token status indicator
            listing.Gap(8f);
            if (!string.IsNullOrEmpty(settings.AccessToken))
            {
                Rect tokenStatusRect = listing.GetRect(20f);
                GUI.color = Color.green;
                // OLD: Widgets.Label(tokenStatusRect, "✓ Token configured - Ready to connect");
                Widgets.Label(tokenStatusRect,
                    "RICS.Twitch.TokenStatus.Ready".Translate());
                GUI.color = Color.white;

                // Show token type - ensure enough space
                Rect tokenTypeRect = listing.GetRect(18f);
                bool isBotToken = !string.IsNullOrEmpty(settings.BotUsername) &&
                                  settings.BotUsername.ToLower() != settings.ChannelName.ToLower();
                // OLD: string tokenType = isBotToken ? "Bot account token" : "Main account token";
                string tokenTypeKey = isBotToken ? "RICS.Twitch.TokenType.Bot" : "RICS.Twitch.TokenType.Main";
                // OLD: Widgets.Label(tokenTypeRect, $"Token type: {tokenType}");
                Widgets.Label(tokenTypeRect, "RICS.Twitch.TokenTypePrefix".Translate() + " " + tokenTypeKey.Translate());
                // OLD: TooltipHandler.TipRegion(tokenTypeRect, "Shows which account this token belongs to");
                TooltipHandler.TipRegion(tokenTypeRect,
                    "RICS.Twitch.TokenTypeTooltip".Translate()
                );
            }
            else
            {
                Rect tokenStatusRect = listing.GetRect(20f);
                GUI.color = Color.red;
                // OLD: Widgets.Label(tokenStatusRect, "❌ No token - Cannot connect to Twitch");
                Widgets.Label(tokenStatusRect,
                    "RICS.Twitch.TokenStatus.Missing".Translate());
                GUI.color = Color.white;
            }

            listing.Gap(20f);

            // Connection Settings
            Text.Font = GameFont.Medium;
            // OLD: listing.Label("Twitch Connection Settings");
            listing.Label("RICS.Twitch.ConnectionSettingsHeader".Translate());  
            Text.Font = GameFont.Small;
            listing.GapLine(6f);
            listing.Gap(4f);

            // Auto-connect with proper tooltip
            Rect autoConnectRect = listing.GetRect(30f);
            // OLD: Widgets.CheckboxLabeled(autoConnectRect, "Auto-connect on startup", ref settings.AutoConnect);
            Widgets.CheckboxLabeled(autoConnectRect,
                "RICS.Twitch.AutoConnectLabel".Translate(),
                ref settings.AutoConnect);
            /* OLD:
            TooltipHandler.TipRegion(autoConnectRect,
                "<b>Auto-connect on Startup</b>\n\n" +
                "When enabled, the mod will automatically connect to\n" +
                "Twitch when RimWorld loads.\n\n" +
                "✅ <b>Good for:</b>\n" +
                "• Regular streamers\n" +
                "• Set-and-forget setup\n\n" +
                "❌ <b>Disable if:</b>\n" +
                "• You only stream occasionally\n" +
                "• Testing different configurations\n" +
                "• Using multiple streaming platforms");
            */

            TooltipHandler.TipRegion(autoConnectRect,
                "RICS.Twitch.AutoConnectTooltip".Translate()
            );

            // Connection status and controls
            listing.Gap(12f);

            if (settings.IsConnected)
            {
                Rect statusRect = listing.GetRect(24f);
                // OLD: Widgets.Label(statusRect, "Status: <color=green>Connected to Twitch</color>");
                string connectedLabel = "RICS.Twitch.ConnectionStatus".Translate() +
                    "<color=green>" + "RICS.Twitch.ConnectedLabel".Translate() + "</color>"; // "Connected to Twitch"
                Widgets.Label(statusRect, connectedLabel); // "Connected to Twitch")
                TooltipHandler.TipRegion(statusRect,
                    "<b>Connected to Twitch</b>\n\n" +
                    "✅ Successfully connected to:\n" +
                    $"• Channel: {settings.ChannelName}\n" +
                    $"• Bot: {settings.BotUsername}\n\n" +
                    "The mod is now listening for chat commands\n" +
                    "and processing viewer interactions.");

                Rect disconnectRect = listing.GetRect(30f);
                if (Widgets.ButtonText(disconnectRect, "Disconnect from Twitch"))
                {
                    CAPChatInteractiveMod.Instance.TwitchService.Disconnect();
                    Messages.Message("Disconnected from Twitch chat", MessageTypeDefOf.SilentInput);
                }
                TooltipHandler.TipRegion(disconnectRect, "Disconnect from Twitch chat");
            }
            else
            {
                Rect statusRect = listing.GetRect(24f);
                Widgets.Label(statusRect, "Status: <color=red>Disconnected</color>");
                TooltipHandler.TipRegion(statusRect,
                    "<b>Disconnected from Twitch</b>\n\n" +
                    "The mod is not currently connected to Twitch chat.\n\n" +
                    "✅ <b>Ready to connect?</b>\n" +
                    "Make sure you have:\n" +
                    "• Channel name entered\n" +
                    "• Valid OAuth token\n" +
                    "• Bot username (optional)\n\n" +
                    "Then click 'Connect to Twitch' below.");

                bool canConnect = settings.CanConnect;
                Rect connectRect = listing.GetRect(30f);

                if (canConnect)
                {
                    if (Widgets.ButtonText(connectRect, "Connect to Twitch"))
                    {
                        CAPChatInteractiveMod.Instance.TwitchService.Connect();
                        Messages.Message("Connecting to Twitch...", MessageTypeDefOf.SilentInput);
                    }
                    TooltipHandler.TipRegion(connectRect, "Connect to Twitch chat");
                }
                else
                {
                    GUI.color = Color.gray;
                    Widgets.ButtonText(connectRect, "Connect to Twitch (Missing Settings)");
                    GUI.color = Color.white;

                    // Show what's missing - with proper spacing
                    listing.Gap(4f);
                    Rect missingRect = listing.GetRect(45f);
                    string missing = "";
                    if (string.IsNullOrEmpty(settings.ChannelName)) missing += "• Channel name\n";
                    if (string.IsNullOrEmpty(settings.AccessToken)) missing += "• OAuth token\n";
                    if (string.IsNullOrEmpty(settings.BotUsername)) missing += "• Bot username\n";

                    if (!string.IsNullOrEmpty(missing))
                    {
                        GUI.color = Color.yellow;
                        Widgets.Label(missingRect, $"Missing requirements:\n{missing}");
                        GUI.color = Color.white;
                        TooltipHandler.TipRegion(missingRect, "Fix these missing settings to connect");
                    }
                }
            }

            // Quick Tips Section
            listing.Gap(24f);
            Text.Font = GameFont.Medium;
            listing.Label("Quick Tips");
            Text.Font = GameFont.Small;
            listing.GapLine(6f);
            listing.Gap(4f);

            Rect tipsRect = listing.GetRect(85f); // More height for tips
            string tips =
                "💡 <b>Common Issues & Solutions:</b>\n" +
                "• <b>Token not working?</b> Regenerate at Twitch Token Generator\n" +
                "• <b>Bot not joining?</b> Check channel name spelling\n" +
                "• <b>Connection drops?</b> Check internet stability\n" +
                "• <b>See your own messages?</b> That's normal with main account";
            Widgets.Label(tipsRect, tips);
            TooltipHandler.TipRegion(tipsRect, "Helpful tips for troubleshooting Twitch connection");

            listing.End();
            Widgets.EndScrollView();
        }
    }
}