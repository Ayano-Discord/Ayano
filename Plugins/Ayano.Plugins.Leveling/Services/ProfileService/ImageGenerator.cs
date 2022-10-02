using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Ayano.Core.Services.Cache;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Path = System.IO.Path;

namespace Ayano.Plugins.Leveling.Services.ProfileService;

public class ImageGenerator : IDisposable
{
    public const string AVATAR_CACHE = "AvatarCache";
    public const string PROFILE_BG = "ProfileBackgrounds";
    public const string PROFILE_CARDS = "ProfileCards";
    public const string PROFILE_CREATION = "ProfileCreation";
    private readonly Font _heavyTitleFont;
    private readonly Random _rnd = new();
    private readonly Font _statsLightFont;
    private readonly Font _statsTinyFont;

    private readonly Image<Rgba64> _templateOverlay;

    public string ImageGenPath =
        "C:\\Users\\aryan\\Documents\\GitHub\\Ayano\\src\\Ayano\\Commands\\Leveling\\Services\\ProfileService";

    public ImageGenerator(IDiscordRestChannelAPI channelApi, HttpClientHelper hch, ICacheService cacheService)
    {
        var pCreationPath = Path.Combine(ImageGenPath!, PROFILE_CREATION);
        // Load template into memory so it's faster in the future :D
        _templateOverlay = Image.Load<Rgba64>(Path.Combine(pCreationPath, "profileTemplate.png"));
        _templateOverlay.Mutate(x => x.Resize(new Size(470, 265)));

        // Add and load fonts
        var fc = new FontCollection();
        var fontHeavy = fc.Add(Path.Combine(pCreationPath, "Fonts\\AvenirLTStd-Heavy.ttf"));
        _heavyTitleFont = new Font(fontHeavy, 20.31f, FontStyle.Bold);
        _statsLightFont = new Font(fontHeavy, 13.4f, FontStyle.Bold);
        _statsTinyFont = new Font(fontHeavy, 10.52f, FontStyle.Bold);

        // Create accessory directories
        CreateImageGenDirectoryIfItDoesntExist(AVATAR_CACHE);
        CreateImageGenDirectoryIfItDoesntExist(PROFILE_BG);
        CreateImageGenDirectoryIfItDoesntExist(PROFILE_CARDS);

        _channelApi = channelApi;
        _hch = hch;
        _cacheService = cacheService;
    }

    private static IDiscordRestChannelAPI _channelApi { get; set; }
    private static ICacheService _cacheService { get; set; }
    private static HttpClientHelper _hch { get; set; }

    public void Dispose()
    {
        _templateOverlay.Dispose();
    }

    private void CreateImageGenDirectoryIfItDoesntExist(string dir)
    {
        var path = Path.Combine(ImageGenPath, dir);
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }

    /// <summary>
    ///     Uses stream to save file to path. This will NOT DISPOSE of the stream
    /// </summary>
    public void ResizeAndSaveImage(Stream stream, string path, Size size, ResizeMode resizeMode = ResizeMode.Crop)
    {
        using var img = Image.Load<Rgba64>(stream);
        img.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = size,
            Mode = resizeMode
        }));
        img.Save(path);
    }

    private static void ResizeAndSaveImage(string pathToImage, string path, Size size,
        ResizeMode resizeMode = ResizeMode.Crop)
    {
        using var img = Image.Load<Rgba64>(pathToImage);
        img.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = size,
            Mode = resizeMode
        }));
        img.Save(path);
    }

    public static async Task<string> SaveFileFromUrl(MessageContext ctx, string? url)
    {
        var imageGenPath =
            "C:\\Users\\aryan\\Documents\\GitHub\\Ayano\\src\\Ayano\\Commands\\Leveling\\Services\\ProfileService";
        var id = ctx.User.ID.Value;

        var imageTemp = Path.Combine(imageGenPath, PROFILE_BG,
            $"{id.ToString()}_temp.png");

        await _hch.DownloadAndSaveFile(new Uri(url!, UriKind.RelativeOrAbsolute), imageTemp);

        ResizeAndSaveImage(
            imageTemp,
            Path.Combine(imageGenPath, PROFILE_BG, $"{id.ToString()}.png"),
            new Size(470, 265));

        File.Delete(imageTemp);
        return Path.Combine(imageGenPath, PROFILE_BG, $"{id.ToString()}.png");
    }

    //Its probably really ineffecient but It works
    private static async Task<string> GetImagePath(MessageContext context)
    {
        var id = context.User.ID;
        var imageGenPath =
            "C:\\Users\\aryan\\Documents\\GitHub\\Ayano\\src\\Ayano\\Commands\\Leveling\\Services\\ProfileService";

        var data = _cacheService.Get($"bg:{id.Value}");


        if (!data.HasValue) return Path.Combine(imageGenPath, "ProfileCreation\\Backgrounds", "defaultBG2.png");

        var url = data.Some().ToString();

        try
        {
            int.TryParse(url, out var num);

            if (num != 0)
                return Path.Combine(imageGenPath, "ProfileCreation\\Backgrounds", $"defaultBG{num.ToString()}.png");

            return SaveFileFromUrl(context, url).Result;
        }
        catch (FormatException)
        {
            try
            {
                // Download and resize image
                return SaveFileFromUrl(context, url).Result;
            }
            catch (Exception)
            {
                await _channelApi.CreateMessageAsync(context.ChannelID,
                    "I was unable to download your custom background. This could be due to the image no longer existing. I will use the default one for now.");
                return Path.Combine(imageGenPath, "ProfileCreation\\Backgrounds", "defaultBG2.png");
                // (e + $"Failed to download background image for {id.Value.ToString()}");
            }
        }
    }

    public void GenerateProfileImage(MessageContext ctx, ProfileImageGenDto conf, string outputPath)
    {
        // so as to be able to use both default a custom, we are going to need to create an extra function which will check the
        // cache for a custom image or default, if its the latter we don't download anything and
        // simply return a string with the path to the file. Otherwise we download the image from the url and then return the path.

        var backgroundPath = GetImagePath(ctx).Result;

        var avatarPath = Path.Combine(ImageGenPath, AVATAR_CACHE, $"{conf.UserId.ToString()}.png");

        using var image = new Image<Rgba64>(470, 265);
        DrawProfileBackground(backgroundPath, image, new Size(470, 265));
        DrawProfileAvatar(avatarPath, image, new Rectangle(7, 6, 42, 42), 21);
        // Draw template
        image.Mutate(x => x.DrawImage(_templateOverlay, 1.0f));
        DrawStats(image, conf);
        image.Save(outputPath);
    }

    private TextOptions OptionGenerator(Font font, int originX, int originY, VerticalAlignment vAlign,
        HorizontalAlignment hAlign)
    {
        TextOptions options = new(font)
        {
            Origin = new PointF(originX, originY),
            VerticalAlignment = vAlign,
            HorizontalAlignment = hAlign
        };

        return options;
    }

    private void DrawStats(Image<Rgba64> image, ProfileImageGenDto c)
    {
        var blueHighlight = new Rgba64(47, 166, 222, 1);

        // Draw global Rank
        var globalStatsText = $"GLOBAL RANK: {c.GlobalRank.ToString()}";
        TextOptions renderOps = new(_statsLightFont);

        var textSize = TextMeasurer.Measure(globalStatsText, renderOps);

        if (string.IsNullOrWhiteSpace(c.ClanName))
        {
            // Draw Username
            image.Mutate(x => x.DrawText(
                OptionGenerator(_heavyTitleFont, 60, 27, VerticalAlignment.Center, HorizontalAlignment.Left),
                c.Name,
                Color.White
            ));
        }
        else
        {
            // Draw Username
            image.Mutate(x => x.DrawText(
                OptionGenerator(_heavyTitleFont, 60, 16, VerticalAlignment.Center, HorizontalAlignment.Left),
                c.Name,
                Color.White
            ));
            // Draw Clan name
            image.Mutate(x => x.DrawText(
                OptionGenerator(_statsLightFont, 60, 36, VerticalAlignment.Center, HorizontalAlignment.Left),
                c.ClanName,
                Color.White
            ));
        }


        image.Mutate(x => x.DrawText(
            OptionGenerator(_statsLightFont, 235 - (int)textSize.Width / 2, 221, VerticalAlignment.Center,
                HorizontalAlignment.Left),
            "GLOBAL RANK: ",
            Color.White
        ));

        image.Mutate(x => x.DrawText(
            OptionGenerator(_statsLightFont, 235 + (int)textSize.Width / 2, 221, VerticalAlignment.Center,
                HorizontalAlignment.Right),
            c.GlobalRank.ToString(),
            // _statsLightFont,
            blueHighlight
        // new Vector2(235 + (textSize.Width / 2), 221)));
        ));

        // Draw global level
        image.Mutate(x =>
            x.DrawText(
                OptionGenerator(_statsLightFont, 235, 236, VerticalAlignment.Center, HorizontalAlignment.Center),
                $"LEVEL: {c.GlobalLevel.ToString()}",
                // _statsLightFont
                Color.White
            // new Vector2(235, 236)));
            ));
        // Draw global EXP
        image.Mutate(x =>
            x.DrawText(
                OptionGenerator(_statsTinyFont, 235, 250, VerticalAlignment.Center, HorizontalAlignment.Center),
                $"{c.GlobalExp.ToString()} / {c.GlobalNextLevelExp.ToString()}",
                // _statsTinyFont,
                Color.White
            // new Vector2(235, 250)));
            ));

        // Draw local STats
        var localStatsText = $"LOCAL RANK: {c.LocalRank.ToString()}";
        var localTextSize = TextMeasurer.Measure(localStatsText, renderOps);

        image.Mutate(x =>
            x.DrawText(
                OptionGenerator(_statsLightFont, 386 - (int)localTextSize.Width / 2, 221, VerticalAlignment.Center,
                    HorizontalAlignment.Left),
                "LOCAL RANK: ",
                // _statsLightFont,
                Color.White
            // new Vector2(386 - (localTextSize.Width / 2), 221)));
            ));
        image.Mutate(x =>
            x.DrawText(
                OptionGenerator(_statsLightFont, 386 + (int)localTextSize.Width / 2, 221, VerticalAlignment.Center,
                    HorizontalAlignment.Right),
                c.LocalRank.ToString(),
                // _statsLightFont,
                blueHighlight
            // new Vector2(386 + (localTextSize.Width / 2), 221)));
            ));
        image.Mutate(x =>
            x.DrawText(
                OptionGenerator(_statsLightFont, 386, 236, VerticalAlignment.Center, HorizontalAlignment.Center),
                $"LEVEL: {c.LocalLevel.ToString()}",
                // _statsLightFont,
                Color.White
            // new Vector2(386, 236)));
            ));
        image.Mutate(x =>
            x.DrawText(
                OptionGenerator(_statsTinyFont, 386, 250, VerticalAlignment.Center, HorizontalAlignment.Center),
                $"{c.LocalExp.ToString()} / {c.LocalNextLevelExp.ToString()}",
                // _statsTinyFont,
                Color.White
            // new Vector2(386, 250)));
            ));
    }

    private void DrawProfileAvatar(string avatarPath, Image<Rgba64> image, Rectangle pos, int cornerRadius)
    {
        using var avatar = Image.Load<Rgba64>(avatarPath);

        avatar.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Crop,
            Size = pos.Size
        }).ApplyRoundedCorners(cornerRadius));


        // avatar.Mutate(x => x.Resize(pos.Size));
        // ReSharper disable once AccessToDisposedClosure
        image.Mutate(x => x.DrawImage(avatar, pos.Location, 1));
    }


    private void DrawProfileBackground(string backgroundPath, Image<Rgba64> image, Size size)
    {
        using var bg = Image.Load(backgroundPath); // 960x540
        bg.Mutate(x => x.Resize(size));
        // ReSharper disable once AccessToDisposedClosure
        image.Mutate(x => x.DrawImage(bg, 1.0f));
    }
}

internal static class ImageGenerationExtension
{
    public static IImageProcessingContext ApplyRoundedCorners(this IImageProcessingContext ctx, float cornerRadius)
    {
        var size = ctx.GetCurrentSize();
        var corners = BuildCorners(size.Width, size.Height, cornerRadius);

        ctx.SetGraphicsOptions(new GraphicsOptions
        {
            Antialias = true,
            AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
        });

        return ctx.Fill(Color.Red, corners);
    }

    private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
    {
        // Create a square
        var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

        // then cut out of the square a circle so we are left with a corner
        var cornerTopLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

        // corner is now positioned in the top left
        // let's make 3 more positioned correctly, we do that by translating the top left one
        // around the center of the image
        // center = width / 2, height /2

        var rightPos = imageWidth - cornerTopLeft.Bounds.Width + 1;
        var bottomPos = imageHeight - cornerTopLeft.Bounds.Height + 1;

        // move it across the width of the image / shape
        var cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
        var cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
        var cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);

        return new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
    }
}