using System.ComponentModel;
using System.Drawing;
using Ayano.Plugins.Fun.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using RemoraResult = Remora.Results.Result;

namespace Ayano.Plugins.Fun.Commands;

public class HackCommand: CommandGroup
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly MessageContext _context;

    public HackCommand(
        IDiscordRestChannelAPI channelApi,
        MessageContext context
    )
    {
        _channelApi = channelApi;
        _context = context;
    }
    
    [Command("hack", "funnyhacck", "dox")]
    [Description("Run some sus hacks on a user...")]
    public async Task<RemoraResult> Hack(IGuildMember member)
    {
        if (member.User.Value.ID.Value == 763633584677978112)
        {
            await _channelApi.CreateMessageAsync(_context.ChannelID, "Thou shan't hack me mortal...");
        }
        else if (member.User.Value.ID == _context.User.ID)
        {
            await _channelApi.CreateMessageAsync(_context.ChannelID,
                "I mean if you want to hack yourself just burn your computer...");
        }
        else
        {
            try
            {
                await _channelApi.DeleteMessageAsync(_context.ChannelID, _context.MessageID,
                    "Doxing users... (Joke Command)");
            }
            catch (Exception) { }

            var message = await _channelApi.CreateMessageAsync(_context.ChannelID,
                $"Attempting to hack <@{member.User.Value.ID}>...");

            var rnd = new Random();
            var y = rnd.Next(1000, 10000);
            var userName = member.Nickname.Value ?? member.User.Value.Username;

            string[] texts =
            {
                $"Collecting sensitive information ⚠️from past attacks on {userName}... ",
                $"Launching Malware ☣️attacks on {userName} !!",
                $"Injecting ransware and pegasus into {userName}'s system 👾👾 👾 ",
                $"Encrypting your important files 🔑🔐.....Making it unreadable to {userName} 🖾🖾🖾",
                $"Launching Brute-Force-Attack and adding {userName}'s ip address to botnets!!! 📍📍📍",
                $"Selling {userName}'s sensitive data to ha*** 🎭 and got a profit worth {y} dollars 🤑 ",
                $"The dangerous hack has been completed and <@{member.User.Value.ID}>'s system has been filled with viruses 💀💀💀!!!"
            };

            foreach (var i in texts)
            {
                await Task.Delay(2000);
                await _channelApi.EditMessageAsync(_context.ChannelID, message.Entity.ID, i);
            }

            string[] salaryArray =
            {
                "Retard makes no money LOL",
                $"${rnd.Next(0, 1000)}",
                "<$50,000",
                "<$75,000",
                "$100,000",
                "$125,000",
                "$150,000",
                "$175,000",
                "$200,000+"
            };

            var name = rnd.Next(Utils.name.Length);
            var age = rnd.Next(1, 101);
            var weight = $"{rnd.Next(60, 400)} lbs";
            var location = rnd.Next(Utils.location.Length);
            var ethnicity = rnd.Next(Utils.ethnicity.Length);
            var religion = rnd.Next(Utils.religion.Length);
            var sexuality = rnd.Next(Utils.sexuality.Length);
            var education = rnd.Next(Utils.education.Length);
            var occupation = rnd.Next(Utils.name.Length);
            var hairColor = rnd.Next(Utils.hair_color.Length);
            var salary = rnd.Next(salaryArray.Length);
            var email = rnd.Next(Utils.email.Length);

            var embed = new Embed
            {
                Colour = Color.Crimson,
                Title = "Hack completed successfully!",
                Fields = new[]
                {
                    new EmbedField(
                        "Extracted Data:",
                        $"Successfully hacked {userName}\n" +
                        $"Name: {Utils.name[name]}\n" +
                        $"Age: {age}\nWeight: {weight}\n" +
                        $"Location: {Utils.location[location]}\n" +
                        $"Ethnicity: {Utils.ethnicity[ethnicity]}\n" +
                        $"Religion: {Utils.religion[religion]}\n" +
                        $"Sexuality: {Utils.sexuality[sexuality]}\n" +
                        $"Education: {Utils.education[education]}\n" +
                        $"Occupation: {Utils.occupation[occupation]}\n" +
                        $"Hair Colour: {Utils.hair_color[hairColor]}\n" +
                        $"Salary: {salaryArray[salary]}\n" +
                        //  $"Likes: {random.choices(likes, k = 3)}\n" +
                        $"Email: {member.Nickname}{Utils.email[email]}\n"
                    )
                }
            };

            await Task.Delay(1500);

            return (RemoraResult)await _channelApi.EditMessageAsync(_context.ChannelID, message.Entity.ID, string.Empty, new[] { embed });
        }

        return RemoraResult.FromError<string>("Something went wrong...");
    }
}