using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Text;
using System.Threading;
using Discord.Commands;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using CefSharp.OffScreen;
using CefSharp;
using SurvivioData;

namespace Testbot2
{
    public class ChromeDriver
    {
        public static void Main(string[] args) => new ChromeDriver().MainAsync().GetAwaiter().GetResult();
        ExportCollection data;

        public async Task MainAsync()
        {
            DiscordSocketClient _client = new DiscordSocketClient();
            _client.Log += Log;

            string token = "NDUzNjUzNjU0OTg2MDMxMTA0.DfiBcg.d6AWe7EHygTPqY8hMR6b0dfmqBM";
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await _client.SetGameAsync("Surviv.io");
            data = ExportReader.Read();

            _client.MessageReceived += MessageReceived;

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage message)
        {
            List<String> weapons = new List<string>();
            data.Find("bullets").Children.ForEach(c => weapons.Add(c.Name));

            if (message.Content == "?s")
            {
                await Task.Factory.StartNew(async () =>
                 {
                     await message.Channel.SendMessageAsync("Generating lobby please wait ..");
                     EmbedBuilder build = CommandS();
                     await message.Channel.SendMessageAsync("", false, build);
                 });
            }
            else if (message.Content == "?weapons")
            {
                EmbedBuilder build = CommandWeapons();
                await message.Channel.SendMessageAsync("", false, build);
            }
            else if (weapons.Any(x => x.Substring(7) == message.Content))
            {
                EmbedBuilder build = CommandWeapons(message.Content);
                await message.Channel.SendMessageAsync("", false, build);
            }
            else if (message.Content == "?stats")
            {
                EmbedBuilder build = new EmbedBuilder();
                build.WithDescription("Under development")
                    .WithTitle("[stats]")
                    .WithColor(Color.DarkBlue);
                await message.Channel.SendMessageAsync("", false, build);
            }
            else if (message.Content.Contains("?jakob"))
            {
                EmbedBuilder build = CommandJakob();
                await message.Channel.SendMessageAsync("", false, build);
            }
            else if (message.Content == "?help")
            {
                EmbedBuilder build = CommandHelp();
                await message.Channel.SendMessageAsync("", false, build);
            }
            else if (message.Content == "?r")
            {
                EmbedBuilder build = Left();
                await message.Channel.SendMessageAsync("", false, build);
            }
        }

        #region Commands
        public EmbedBuilder CommandS()
        {
            string s;
            using (ChromiumWebBrowser web = new ChromiumWebBrowser("http://surviv.io/"))
            {
                while (!web.IsBrowserInitialized)
                {
                    Thread.Sleep(100);
                }

                while (web.IsLoading)
                {
                    Thread.Sleep(100);
                }

                web.GetMainFrame().ExecuteJavaScriptAsync("document.getElementById('btn-create-team').click();");

                while (web.Address == "http://surviv.io/")
                {
                    Thread.Sleep(100);
                }
                s = web.Address;
            }

            EmbedBuilder build = new EmbedBuilder();

            build.WithTitle("We play game yes")
                .WithDescription(s)
                .WithColor(Color.DarkBlue);

            return build;
        }

        public EmbedBuilder CommandJakob()
        {
            Random random = new Random();
            int i = random.Next(2);
            string answer = i == 1 ? "Ja" : "Nej";

            EmbedBuilder build = new EmbedBuilder();

            build.WithTitle("hvadsigerjakob.dk v2")
                 .WithDescription("Jakob siger: " + answer)
                 .WithColor(Color.DarkBlue);

            return build;
        }

        private EmbedBuilder CommandWeapons()
        {
            EmbedBuilder build = new EmbedBuilder();
            #region weapon string
            string s = "To get more information about a weapon type ?[WeaponName]" +
                "\n**MP5**" +
                "\n**AK-47**" +
                "\n**SCAR**" +
                "\n**Mosin(kar98)**" +
                "\n**SV-98(AWP)**" +
                "\n**M-39**" +
                "\n**Shotgun**" +
                "\n**M9**" +
                "\n**OT-39**" +
                "\n**Deagle**" +
                "\n**MAC-10**" +
                "\n**UMP-9**" +
                "\n**vector**" +
                "\n**DP-28**" +
                "\n**glock**" +
                "\n**famas**" +
                "\n**HK416**" +
                "\n**MK12**" +
                "\n**M249**";
            #endregion

            build.WithTitle("Survivio weapons")
                .WithDescription(s)
                .WithColor(Color.DarkBlue);

            return build;
        }

        private EmbedBuilder CommandHelp()
        {
            EmbedBuilder Build = new EmbedBuilder();

            Build.WithTitle("**Help**")
               .WithDescription("Below you find a list of all actions performable by the Bot." +
               "\n" +
               "\n_**Surviv.io commands**_" +
               "\n" +
               "\n**?s**" +
               "\nCreates a new surviv.io lobby after a brief delay." +
               "\n" +
               "\n**?weapons**" +
               "\nReturn the full list of Surviv.io weapons. (In development)" +
               "\n" +
               "\n**?[WeaponName]**" +
               "\nReturn a weapons damage, stats and dps. (In development)" +
               "\n" +
               "\n**?stats [name]**" +
               "\nRetrieves your surviv.io stats. (In development)" +
               "\n" +
               "\n_**Additional functionality**_" +
               "\n" +
               "\n**?jakob**" +
               "\nJakob replis with a yes or no. Your question can be written after ?jakob")
               .WithColor(Color.DarkMagenta);

            return Build;
        }

        private EmbedBuilder Left()
        {
            EmbedBuilder build = new EmbedBuilder();
           
            DateTime turnInTime = new DateTime(2018, 6, 20, 8, 30, 0);
            TimeSpan timeRemaining = turnInTime.Subtract(DateTime.Now);
            string timeRemainingString = timeRemaining.Days + " days, " + timeRemaining.Hours + " hours, " + timeRemaining.Minutes + " minutes";

            build.WithTitle("We got da exam in")
                .WithDescription(timeRemainingString)
                .WithColor(Color.Purple);

            return build;
        }
        private EmbedBuilder CommandWeapons(string content)
        {
            EmbedBuilder build = new EmbedBuilder();
            ExportCollection weapon_items = data.Find("items", content);
            ExportCollection weapon_bullets = data.Find("bullets", "bullet_" + content);

            build.WithTitle(content)
                .WithDescription("Damage per hit: " + weapon_bullets.Double("damage")  +
                "\nObstacle damage modifier: " + weapon_bullets.Double("obstacleDamage") +
                "\nFalloff(range): " + weapon_bullets.Double("falloff") +
                "\nDistance: " + weapon_bullets.Double("distance") +
                "\nProjectile speed: " + weapon_bullets.Double("speed") +
                "\nAmmo type: + " + weapon_items.String("ammo"))
                .WithColor(Color.DarkBlue);

            return build;
        }
        #endregion
    }
}
