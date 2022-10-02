namespace Ayano.Plugins.Leveling.Services.ProfileService;

public class HttpClientHelper : IDisposable
{
    public HttpClientHelper()
    {
        HttpClient = new HttpClient();
    }

    public HttpClient HttpClient { get; }

    public void Dispose()
    {
        HttpClient?.Dispose();
    }

    /// <summary>
    ///     Downloads a File and saves it to the given path.
    ///     Might throw an exception if there's a failure at any stage
    /// </summary>
    public async Task DownloadAndSaveFile(Uri url, string path)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var resp = await HttpClient.SendAsync(request).ConfigureAwait(false);

        await using var contentStream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await using var stream = new FileStream(path, FileMode.Create,
            FileAccess.Write, FileShare.None, 3145728, true);

        await contentStream.CopyToAsync(stream).ConfigureAwait(false);
        await contentStream.FlushAsync().ConfigureAwait(false);
        await stream.FlushAsync().ConfigureAwait(false);
    }

    public async Task<Stream> DownloadFileAsStream(Uri url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var resp = await HttpClient.SendAsync(request).ConfigureAwait(false);
        return await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
    }
}