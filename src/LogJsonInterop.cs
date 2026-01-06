using Microsoft.JSInterop;
using Soenneker.Asyncs.Initializers;
using Soenneker.Blazor.LogJson.Abstract;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.LogJson;

///<inheritdoc cref="ILogJsonInterop"/>
public sealed class LogJsonInterop : ILogJsonInterop
{
    private const string _modulePath = "Soenneker.Blazor.LogJson/js/logjsoninterop.js";
    private const string _moduleIdentifier = "LogJsonInterop";

    private readonly IJSRuntime _jsRuntime;
    private readonly IResourceLoader _resourceLoader;
    private readonly AsyncInitializer _initializer;

    private const string _logJsonIdentifier = _moduleIdentifier + ".logJson";

    private const int _maxBodyBytes = 64 * 1024;
    private const int _maxBodyChars = 64 * 1024;

    public LogJsonInterop(IJSRuntime jSRuntime, IResourceLoader resourceLoader)
    {
        _jsRuntime = jSRuntime;
        _resourceLoader = resourceLoader;
        _initializer = new AsyncInitializer(Initialize);
    }

    private ValueTask Initialize(CancellationToken token)
    {
        return _resourceLoader.ImportModuleAndWaitUntilAvailable(_modulePath, _moduleIdentifier, 100, token);
    }

    public ValueTask Log<T>(T? value, string group, string level = "log", CancellationToken cancellationToken = default)
    {
        if (value is null)
            return LogObjectInternal(null, group, level, cancellationToken);

        switch (value)
        {
            case string s:
                return LogObjectInternal(s, group, level, cancellationToken);
            case JsonElement je:
                return LogObjectInternal(je.GetRawText(), group, level, cancellationToken);
            case JsonDocument jd:
                return LogObjectInternal(jd.RootElement.GetRawText(), group, level, cancellationToken);
            default:
                return LogObjectInternal(value, group, level, cancellationToken);
        }
    }

    private async ValueTask LogObjectInternal(object? value, string group, string logLevel, CancellationToken cancellationToken)
    {
        await _initializer.Init(cancellationToken);
        await _jsRuntime.InvokeVoidAsync(_logJsonIdentifier, cancellationToken, value, group, logLevel);
    }

    public ValueTask LogRequest(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var uri = request.RequestUri is null ? "Uri not set on request object" : request.RequestUri.ToString();
        return LogRequest(uri, request.Content, request.Method, cancellationToken);
    }

    public async ValueTask LogRequest(string requestUri, HttpContent? httpContent = null, HttpMethod? httpMethod = null,
        CancellationToken cancellationToken = default)
    {
        string? contentString = null;

        if (httpContent is not null)
            contentString = await ReadBodyStringSafe(httpContent, cancellationToken);

        var group = httpMethod is null ? $"Request: {requestUri}" : $"Request: {httpMethod} {requestUri}";

        await LogObjectInternal(contentString, group, "log", cancellationToken);
    }

    public async ValueTask LogResponse(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var contentString = await ReadBodyStringSafe(response.Content, cancellationToken);

        var group = response.RequestMessage is not null
            ? $"Response: {response.RequestMessage.Method} {response.RequestMessage.RequestUri} ({response.StatusCode})"
            : $"Response: ({response.StatusCode})";

        await LogObjectInternal(contentString, group, "log", cancellationToken);
    }

    private static async ValueTask<string> ReadBodyStringSafe(HttpContent content, CancellationToken ct)
    {
        // If length is known and huge, avoid reading entire thing.
        var lenBytes = content.Headers.ContentLength;
        if (lenBytes is > _maxBodyBytes)
            return $"(body skipped; Content-Length={lenBytes.Value:n0} bytes)";

        var s = await content.ReadAsStringAsync(ct);

        if (s.Length <= _maxBodyChars)
            return s;

        return string.Concat(s.AsSpan(0, _maxBodyChars), "… (truncated)");
    }


    public async ValueTask DisposeAsync()
    {
        await _resourceLoader.DisposeModule(_modulePath);
        await _initializer.DisposeAsync();
    }

    public void Dispose()
    {
        _resourceLoader.DisposeModule(_modulePath);
        _initializer.Dispose();
    }
}