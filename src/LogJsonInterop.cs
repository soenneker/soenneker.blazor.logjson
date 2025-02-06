using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Soenneker.Blazor.LogJson.Abstract;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.AsyncSingleton;

namespace Soenneker.Blazor.LogJson;

///<inheritdoc cref="ILogJsonInterop"/>
public class LogJsonInterop : ILogJsonInterop
{
    private const string ModulePath = "Soenneker.Blazor.LogJson/logjsoninterop.js";
    private const string ModuleIdentifier = "LogJsonInterop";

    private readonly IJSRuntime _jsRuntime;
    private readonly IResourceLoader _resourceLoader;
    private readonly AsyncSingleton<object> _initializer;
    private readonly ILogger<LogJsonInterop> _logger;

    public LogJsonInterop(IJSRuntime jSRuntime, IResourceLoader resourceLoader, ILogger<LogJsonInterop> logger)
    {
        _jsRuntime = jSRuntime;
        _resourceLoader = resourceLoader;
        _logger = logger;

        _initializer = new AsyncSingleton<object>(async (token, _) =>
        {
            await _resourceLoader.ImportModuleAndWaitUntilAvailable(ModulePath, ModuleIdentifier, 100, token)
                                 .NoSync();

            return new object();
        });
    }

    public async ValueTask LogJson(string? jsonString, string group, string logLevel = "log", CancellationToken cancellationToken = default)
    {
        await _initializer.Get(cancellationToken)
                          .NoSync();

        await _jsRuntime.InvokeVoidAsync("LogJsonInterop.logJson", cancellationToken, jsonString, group, logLevel)
                        .NoSync();
    }

    public ValueTask LogRequest(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        return LogRequest(request.RequestUri?.ToString() ?? "Uri not set on request object", request.Content, request.Method, cancellationToken);
    }

    public async ValueTask LogRequest(string requestUri, HttpContent? httpContent = null, HttpMethod? httpMethod = null, CancellationToken cancellationToken = default)
    {
        var contentString = httpContent is not null
            ? await httpContent.ReadAsStringAsync(cancellationToken)
                               .NoSync()
            : null;

        var group = httpMethod is not null ? $"Request: {httpMethod} {requestUri}" : $"Request: {requestUri}";

        await LogJson(contentString, group, cancellationToken: cancellationToken)
            .NoSync();
    }

    public async ValueTask LogResponse(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var contentString = await response.Content.ReadAsStringAsync(cancellationToken)
                                          .NoSync();

        var group = response.RequestMessage is not null
            ? $"Response: {response.RequestMessage.Method} {response.RequestMessage.RequestUri} ({response.StatusCode})"
            : $"Response: ({response.StatusCode})";

        await LogJson(contentString, group, cancellationToken: cancellationToken)
            .NoSync();
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogDebug("Disposing LogJsonInterop.");

        await _resourceLoader.DisposeModule("Soenneker.Blazor.LogJson/logjsoninterop.js")
                             .NoSync();

        await _initializer.DisposeAsync()
                          .NoSync();

        GC.SuppressFinalize(this);
    }
}