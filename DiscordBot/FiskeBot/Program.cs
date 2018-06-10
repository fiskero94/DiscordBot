using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace FiskeBot
{
    class Program
    {
        static void Main(string[] args) => new Program().RunAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task RunAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            _client.Log += Log;
            await RegisterCommandsAsync();
            string token = "NDU1MTc3ODI0MDEwMjQwMDAw.Df4QLQ.UaVjC7m7zRJtjJQvXru0fG-tF9k";
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }
        
        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage msg)
        {
            SocketUserMessage message = msg as SocketUserMessage;
            if (message is null || message.Author.IsBot) return;
            int argPos = 0;
            if (message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                SocketCommandContext context = new SocketCommandContext(_client, message);
                IResult result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess)
                    Console.WriteLine(result.ErrorReason);
            }
        }

        private Task Log(LogMessage message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }
    }
}