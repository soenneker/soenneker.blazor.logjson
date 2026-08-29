using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.LogJson.Abstract;

/// <summary>
/// A Blazor interop library that logs JSON (like HTTP requests/responses) within the browser
/// </summary>
public interface ILogJsonInterop : IAsyncDisposable
{
    /// <summary>
    /// Serializes a value and writes it to the selected browser-console group and level.
    /// </summary>
    /// <typeparam name="T">Type of value handled by the log json.</typeparam>
    /// <param name="value">Value to serialize and write to the browser console.</param>
    /// <param name="group">Group to target.</param>
    /// <param name="level">Logging level used for the console entry.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the log operation is complete.</returns>
    ValueTask Log<T>(T? value, string group, string level = "log", CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an HTTP request with its URI, content, and method.
    /// </summary>
    /// <param name="request">request that defines the request to send.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the log request operation is complete.</returns>
    ValueTask LogRequest(HttpRequestMessage request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an HTTP request with its URI, content, and method.
    /// </summary>
    /// <param name="requestUri">request URI that defines the request to send.</param>
    /// <param name="httpContent">The content of the HTTP request. Can be null.</param>
    /// <param name="httpMethod">The method of the HTTP request (e.g., GET, POST). Can be null.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the log request operation is complete.</returns>
    ValueTask LogRequest(string requestUri, HttpContent? httpContent = null, HttpMethod? httpMethod = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an HTTP response.
    /// </summary>
    /// <param name="response">response returned by the upstream operation.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the log response operation is complete.</returns>
    ValueTask LogResponse(HttpResponseMessage response, CancellationToken cancellationToken = default);
}
