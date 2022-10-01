// using Remora.Discord.API.Abstractions.Gateway.Events;
// using Remora.Discord.API.Abstractions.Rest;
// using Remora.Discord.Gateway.Responders;
// using Remora.Results;
// using Ayano.Core.Services;

// namespace Ayano.Core.Responders;

// public class LevelingResponder : IResponder<IMessageCreate>
// {
//     private readonly IDiscordRestChannelAPI _discordRestChannelApi;
//     private readonly IDiscordRestUserAPI _discordRestUserApi;
//     private readonly Database _db;

//     public LevelingResponder(
//         IDiscordRestUserAPI discordRestUserApi,
//         IDiscordRestChannelAPI discordRestChannelApi,
//         Database db)
//     {
//         _discordRestUserApi = discordRestUserApi;
//         _discordRestChannelApi = discordRestChannelApi;
//         _db = db;

//     }

//     public async Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct = default)
//     {
//         Console.WriteLine(((uint)_db.StringGetAsync($"user:{gatewayEvent.Author.ID}:exp").Result));

//         if (!gatewayEvent.Author.IsBot.Value)
//         {
//             var user = gatewayEvent.Author;
//             var channel = await _discordRestChannelApi.GetChannelAsync(gatewayEvent.ChannelID, ct);
//             var expCooldown = await _db.StringGetAsync($"user:{user.ID}:expCooldown");
//             var oldExp = await _db.StringGetAsync($"user:{user.ID}:exp");
//             if (((ulong)expCooldown > ((ulong)DateTime.UtcNow.Millisecond) || expCooldown.IsNullOrEmpty)) {
//                 Random rnd = new Random();
//                 int exp = rnd.Next(11);
//                 await _db.StringSetAsync($"user:{user.ID}:exp", ((int)oldExp + exp).ToString());
//                 await _db.StringSetAsync($"user:{user.ID}:expCooldown", DateTime.UtcNow.Millisecond + 60000);
//                 return Result.FromSuccess();
//             } else {
//                 return Result.FromSuccess();
//             }
//         }
//         return Result.FromSuccess();
//     }
// }