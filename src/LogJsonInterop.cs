using Microsoft.JSInterop;
using Soenneker.Blazor.LogJson.Abstract;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.AsyncSingleton;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.LogJson;

///<inheritdoc cref="ILogJsonInterop"/>
public sealed class LogJsonInterop : ILogJsonInterop
{
    private const string ModulePath = "Soenneker.Blazor.LogJson/js/logjsoninterop.js";
    private const string ModuleIdentifier = "LogJsonInterop";

    private readonly IJSRuntime _jsRuntime;
    private readonly IResourceLoader _resourceLoader;
    private readonly AsyncSingleton _initializer;

    public LogJsonInterop(IJSRuntime jSRuntime, IResourceLoader resourceLoader)
    {
        _jsRuntime = jSRuntime;
        _resourceLoader = resourceLoader;

        _initializer = new AsyncSingleton(async (token, _) =>
        {
            await _resourceLoader.ImportModuleAndWaitUntilAvailable(ModulePath, ModuleIdentifier, 100, token);

            return new object();
        });
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
        // NOTE: We pass the object, not a string. The JS above handles non-strings cleanly.
        await _jsRuntime.InvokeVoidAsync($"{ModuleIdentifier}.logJson", cancellationToken, value, group, logLevel);
    }

    public ValueTask LogRequest(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        return LogRequest(request.RequestUri?.ToString() ?? "Uri not set on request object", request.Content, request.Method, cancellationToken);
    }

    public async ValueTask LogRequest(string requestUri, HttpContent? httpContent = null, HttpMethod? httpMethod = null,
        CancellationToken cancellationToken = default)
    {
        var contentString = httpContent is not null ? await httpContent.ReadAsStringAsync(cancellationToken) : null;

        var group = httpMethod is not null ? $"Request: {httpMethod} {requestUri}" : $"Request: {requestUri}";

        await LogObjectInternal(contentString, group, "log", cancellationToken: cancellationToken);
    }

    public async ValueTask LogResponse(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var contentString = await response.Content.ReadAsStringAsync(cancellationToken);

        var group = response.RequestMessage is not null
            ? $"Response: {response.RequestMessage.Method} {response.RequestMessage.RequestUri} ({response.StatusCode})"
            : $"Response: ({response.StatusCode})";

        await LogObjectInternal(contentString, group, "log", cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _resourceLoader.DisposeModule(ModulePath);
        await _initializer.DisposeAsync();
    }
}
