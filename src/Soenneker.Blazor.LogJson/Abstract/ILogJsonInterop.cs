using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.LogJson.Abstract;

/// <summary>
/// A Blazor interop library that logs JSON (like HTTP requests/responses) within the browser
/// </summary>
public interface ILogJsonInterop : IAsyncDisposable, IDisposable
{
    ValueTask Log<T>(T? value, string group, string level = "log", CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an HTTP request with its URI, content, and method.
    /// </summary>
    ValueTask LogRequest(HttpRequestMessage request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an HTTP request with its URI, content, and method.
    /// </summary>
    /// <param name="requestUri">The URI of the HTTP request.</param>
    /// <param name="httpContent">The content of the HTTP request. Can be null.</param>
    /// <param name="httpMethod">The method of the HTTP request (e.g., GET, POST). Can be null.</param>
    /// <param name="cancellationToken"></param>
    ValueTask LogRequest(string requestUri, HttpContent? httpContent = null, HttpMethod? httpMethod = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an HTTP response.
    /// </summary>
    /// <param name="response">The HTTP response to log.</param>
    /// <param name="cancellationToken"></param>
    ValueTask LogResponse(HttpResponseMessage response, CancellationToken cancellationToken = default);
}
