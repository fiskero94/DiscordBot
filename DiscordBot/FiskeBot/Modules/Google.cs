using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FiskeBot.Modules
{
    public class Google : ModuleBase<SocketCommandContext>
    {
        [Command("google")]
        public async Task GoogleAsync()
        {
            await ReplyAsync("Hello World!");
        }
    }
}