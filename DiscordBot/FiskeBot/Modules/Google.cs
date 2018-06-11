using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using static Google.Apis.Customsearch.v1.CseResource;
using Google.Apis.Customsearch.v1.Data;

namespace FiskeBot.Modules
{
    public class Google : ModuleBase<SocketCommandContext>
    {
        private EmbedAuthorBuilder Author => new EmbedAuthorBuilder().WithName("Google")
                                                                     .WithUrl("https://www.google.com")
                                                                     .WithIconUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/5/53/Google_%22G%22_Logo.svg/512px-Google_%22G%22_Logo.svg.png");

        private int UsesRemaining
        {
            get
            {
                int? uses = Persistence.Retrieve<int?>("GoogleUsesRemaining");
                if (uses is null)
                {
                    uses = 100;
                    Persistence.Persist("GoogleUsesRemaining", uses);
                }
                return (int)uses;
            }
            set
            {
                Persistence.Persist("GoogleUsesRemaining", value);
            }
        }

        [Command("g")]
        public async Task GoogleAsync()
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("How to search")
                   .WithDescription("You need to specify your query, you can do so by typing '!g Example Search', this will return the first result. " + 
                                    "If you want more results, you can type '!g 5 Example Search', which will return 5 results. You can retrieve 1-5 results.")
                   .WithColor(Color.Blue)
                   .WithAuthor(Author);
            await ReplyAsync("", false, builder.Build());
        }

        [Command("g")]
        public async Task GoogleAsync([Remainder]string search) => await GoogleAsync(1, search);

        [Command("g")]
        public async Task GoogleAsync(int amount, [Remainder]string search)
        {
            if (amount > 5) amount = 5;
            else if (amount < 1) amount = 1;
            if (Config.GoogleApiKey is null || Config.SearchEngineID is null)
                await ReplyAsync("", false, Error("Missing API Key or Search Engine ID"));
            else if (UsesRemaining < 1)
                await ReplyAsync("", false, Error("No API uses remaining"));
            else
            {
                CustomsearchService searchService = new CustomsearchService(new BaseClientService.Initializer { ApiKey = Config.GoogleApiKey });
                ListRequest request = searchService.Cse.List(search);
                request.Cx = Config.SearchEngineID;
                Search response = request.Execute();
                UsesRemaining -= 1;
                if (response.Items.Count < amount) amount = response.Items.Count;
                if (response.Items.Count == 0) await ReplyAsync("", false, Error("No results"));
                else await ReplyAsync("", false, Results(amount, search, response));
            }
        }

        private Embed Results(int amount, string search, Search response)
        {
            string description = search;
            foreach (Result result in response.Items)
            {
                if (amount > 0)
                    description += "\n\n**[" + result.Title + "](" + result.Link + ")**\n" + result.Snippet;
                else break;
                amount--;
            }
            
            EmbedFooterBuilder footer = new EmbedFooterBuilder();
            footer.WithText(UsesRemaining + " Uses Remaining | " + 
                            String.Format("{0:n0}", response.SearchInformation.TotalResults) + " Results, " + 
                            ((double)response.SearchInformation.SearchTime).ToString("N2") + " Seconds");

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Searching...")
                   .WithDescription(description)
                   .WithColor(Color.Green)
                   .WithFooter(footer)
                   .WithAuthor(Author);
            return builder.Build();
        }

        private Embed Error(string message)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Something went wrong")
                   .WithDescription(message)
                   .WithColor(Color.Red)
                   .WithAuthor(Author);
            return builder.Build();
        }
    }
}