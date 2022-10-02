using Ayano.Core.Services.Cache;
using Ayano.Core.Responders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.Gateway;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.API.Objects;
using Microsoft.Extensions.Options;
using Prometheus;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Hosting.Extensions;
using Remora.Plugins.Services;
using Sentry;
using StackExchange.Redis;
using VTP.Remora.Commands.HelpSystem;
using Npgsql;

namespace Ayano;

/// <summary>
///     Represents the main class of the program.
/// </summary>
public class Program
{
    private static readonly IServiceProvider Services = ConfigureServices();

    /// <summary>
    ///     The main entrypoint of the program.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous program execution.</returns>
    public static void Main(string[] args)
    {
        InitAsync(args)
            .GetAwaiter()
            .GetResult();
    }

    private static async Task InitAsync(string[] args)
    {
        using (
            SentrySdk.Init(o =>
                {
                    // Tells which project in Sentry to send events to:
                    o.Dsn = "https://ac5f5c47cd7d487db8cbb13e900bfbc3@o551313.ingest.sentry.io/6432762";
                    // Set traces_sample_rate to 0.5 to capture 50% of transactions for performance monitoring.
                    o.TracesSampleRate = 0.5;
                    o.Release = "0.0.1";
                }
            )
        )
        {
            SetBlankCache();

            var cache = Services.GetRequiredService<ICacheService>();

            Console.WriteLine(cache.Get<int>("global:guildCount").Some());

            var gatewayClient = Services.GetRequiredService<DiscordGatewayClient>();

            var metrics = new KestrelMetricServer(6000);

            try { metrics.Start(); } catch { /* ignored */ }

            var gatewayApi = Services.GetRequiredService<Remora.Discord.API.Abstractions.Rest.IDiscordRestGatewayAPI>();
            var getGatewayEndpoint = await gatewayApi.GetGatewayBotAsync();
            if (!getGatewayEndpoint.IsSuccess)
            {
                Console.WriteLine(getGatewayEndpoint.Error);
            }

            await gatewayClient.RunAsync(new CancellationToken());

            await metrics.StopAsync();

        }
    }

    private static IServiceProvider ConfigureServices()
    {

        var pluginOptions = Options.Create(new PluginServiceOptions(Array.Empty<string>()));
        var pluginService = new PluginService(pluginOptions);

        var plugins = pluginService.LoadPluginTree();

        var connString = "Host=localhost;Username=postgres;Password=postgres;Database=ayano";

        var conn = new NpgsqlConnection(connString);

        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(configuration =>
            {
                configuration.SetBasePath(Directory.GetCurrentDirectory());
                configuration.AddJsonFile("appSettings.json", true, false);
                configuration.AddUserSecrets("Onii-Chan-Ayano", false);
            })
            .AddDiscordService
            (
                _ => "NzYzNjMzNTg0Njc3OTc4MTEy.GbMh_Z.DquT0CdEPlUJphmUlPo8p2ft726xJmHgX6I4zk"// s.GetService<IOptions<AyanoConfigurationOptions>>()!.Value.Discord.BotToken
            )
            .ConfigureServices
            (
                (_, services) =>
                {
                    services.Configure<DiscordGatewayClientOptions>(
                        g =>
                        {
                            g.Intents |= GatewayIntents.MessageContents;
                            g.Presence = new UpdatePresence(ClientStatus.Online, false, DateTimeOffset.Now,
                                new[] { new Activity("%help | @Ayano", ActivityType.Listening) });
                            g.ShardIdentification = new ShardIdentification(0, 1);
                        });

                    services.Configure<HelpSystemOptions>(options =>
                    {
                        options.CommandCategories.Add("Info");
                        options.CommandCategories.Add("Leveling");
                        options.CommandCategories.Add("Fun");
                        options.CommandCategories.Add("Economy");
                    });

                    // services.Configure<CommandResponderOptions>(options => {
                    //     options
                    // })

                    services.AddSingleton(pluginService);
                    services.AddSingleton(conn);
                    services.AddHelpSystem();
                    services.AddHttpClient();
                    services.AddSingleton<ICacheService, CacheService>();
                    services.AddSingleton<IShardIdentification>(s => s.GetRequiredService<IOptions<DiscordGatewayClientOptions>>().Value.ShardIdentification!);
                    services.AddResponder<MentionSelfResponder>();
                    services.AddResponder<GuildCreateResponder>();
                    // services.AddResponder<LevelingResponder>();
                    plugins.ConfigureServices(services);
                }
            )
            .ConfigureLogging
            (
                c => c
                    .AddConsole()
                    .AddFilter("System.Net.Http.HttpClient.*.LogicalHandler", LogLevel.Warning)
                    .AddFilter("System.Net.Http.HttpClient.*.ClientHandler", LogLevel.Warning)
            )
            .Build();

        return host.Services;
    }

    private static void SetBlankCache()
    {
        var cache = Services.GetRequiredService<ICacheService>();
        cache.Set("global:guildCount", 0);
        cache.Set("global:userCount", 0);
        cache.Set("global:channel:totalChannelCount", 0);
    }
}