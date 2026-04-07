using Microsoft.JSInterop;
using Soenneker.Blazor.LogJson.Abstract;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Extensions.CancellationTokens;
using Soenneker.Utils.CancellationScopes;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.LogJson;

///<inheritdoc cref="ILogJsonInterop"/>
public sealed class LogJsonInterop : ILogJsonInterop
{
    private const string _modulePath = "/_content/Soenneker.Blazor.LogJson/js/logjsoninterop.js";

    private const int _maxBodyBytes = 64 * 1024;
    private const int _maxBodyChars = 64 * 1024;

    private readonly IModuleImportUtil _moduleImportUtil;
    private readonly CancellationScope _cancellationScope = new();

    public LogJsonInterop(IModuleImportUtil moduleImportUtil)
    {
        _moduleImportUtil = moduleImportUtil;
    }

    public ValueTask Log<T>(T? value, string group, string level = "log", CancellationToken cancellationToken = default)
    {
        var linked = _cancellationScope.CancellationToken.Link(cancellationToken, out var source);

        using (source)
        {
            if (value is null)
                return LogObjectInternal(null, group, level, linked);

            switch (value)
            {
                case string s:
                    return LogObjectInternal(s, group, level, linked);
                case JsonElement je:
                    return LogObjectInternal(je.GetRawText(), group, level, linked);
                case JsonDocument jd:
                    return LogObjectInternal(jd.RootElement.GetRawText(), group, level, linked);
                default:
                    return LogObjectInternal(value, group, level, linked);
            }
        }
    }

    private async ValueTask LogObjectInternal(object? value, string group, string logLevel, CancellationToken cancellationToken)
    {
        IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, cancellationToken);
        await module.InvokeVoidAsync("logJson", cancellationToken, value, group, logLevel);
    }

    public ValueTask LogRequest(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var uri = request.RequestUri is null ? "Uri not set on request object" : request.RequestUri.ToString();
        return LogRequest(uri, request.Content, request.Method, cancellationToken);
    }

    public async ValueTask LogRequest(string requestUri, HttpContent? httpContent = null, HttpMethod? httpMethod = null,
        CancellationToken cancellationToken = default)
    {
        var linked = _cancellationScope.CancellationToken.Link(cancellationToken, out var source);

        using (source)
        {
            string? contentString = null;

            if (httpContent is not null)
                contentString = await ReadBodyStringSafe(httpContent, linked);

            var group = httpMethod is null ? $"Request: {requestUri}" : $"Request: {httpMethod} {requestUri}";

            await LogObjectInternal(contentString, group, "log", linked);
        }
    }

    public async ValueTask LogResponse(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var linked = _cancellationScope.CancellationToken.Link(cancellationToken, out var source);

        using (source)
        {
            var contentString = await ReadBodyStringSafe(response.Content, linked);

            var group = response.RequestMessage is not null
                ? $"Response: {response.RequestMessage.Method} {response.RequestMessage.RequestUri} ({response.StatusCode})"
                : $"Response: ({response.StatusCode})";

            await LogObjectInternal(contentString, group, "log", linked);
        }
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
        await _moduleImportUtil.DisposeContentModule(_modulePath);
        await _cancellationScope.DisposeAsync();
    }
}
