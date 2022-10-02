using Ayano.Plugins.Info;
using Ayano.Plugins.Info.Commands;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Remora.Commands.Extensions;
using Remora.Plugins.Abstractions;
using Remora.Plugins.Abstractions.Attributes;
using Remora.Results;

[assembly: RemoraPlugin(typeof(InfoPlugin))]

namespace Ayano.Plugins.Info;

public sealed class InfoPlugin : PluginDescriptor
{
    public override string Name => "Info";

    public override string Description => "Contains commands which provide information about Ayano.";

    public override Result ConfigureServices(IServiceCollection serviceCollection)
    {

        var client = new GitHubClient(new ProductHeaderValue("Ayano-Discord-Bot"));

        serviceCollection.AddSingleton(client);

        serviceCollection.AddCommandTree()
            .WithCommandGroup<BotStats>()
            .WithCommandGroup<AboutCommand>();

        return Result.FromSuccess();
    }
}