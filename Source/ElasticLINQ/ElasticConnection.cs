// Licensed under the Apache 2.0 License. See LICENSE.txt in the project root for more information.

using ElasticLinq.Utility;
using ElasticLinq.Logging;
using ElasticLinq.Request;
using ElasticLinq.Response.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ElasticLinq
{
    /// <summary>
    /// Specifies connection parameters for Elasticsearch.
    /// </summary>
    [DebuggerDisplay("{Endpoint.ToString(),nq}{Index,nq}")]
    public class ElasticConnection : BaseElasticConnection, IDisposable
    {
        private readonly string[] parameterSeparator = { "&" };

        /// <summary>
        /// Create a new ElasticConnection with the given parameters defining its properties.
        /// </summary>
        /// <param name="endpoint">The URL endpoint of the Elasticsearch server.</param>
        /// <param name="userName">UserName to use to connect to the server (optional).</param>
        /// <param name="password">Password to use to connect to the server (optional).</param>
        /// <param name="timeout">TimeSpan to wait for network responses before failing (optional, defaults to 10 seconds).</param>
        /// <param name="index">Name of the index to use on the server (optional).</param>
        /// <param name="options">Additional options that specify how this connection should behave.</param>
        public ElasticConnection(Uri endpoint, string userName = null, string password = null, TimeSpan? timeout = null, string index = null, ElasticConnectionOptions options = null)
            : this(new HttpClientHandler(), endpoint, userName, password, index, timeout, options) { }


        /// <summary>
        /// Create a new ElasticConnection with the given parameters for internal testing.
        /// </summary>
        /// <param name="innerMessageHandler">The HttpMessageHandler used to intercept network requests for testing.</param>
        /// <param name="endpoint">The URL endpoint of the Elasticsearch server.</param>
        /// <param name="userName">UserName to use to connect to the server (optional).</param>
        /// <param name="password">Password to use to connect to the server (optional).</param>
        /// <param name="timeout">TimeSpan to wait for network responses before failing (optional, defaults to 10 seconds).</param>
        /// <param name="index">Name of the index to use on the server (optional).</param>
        /// <param name="options">Additional options that specify how this connection should behave.</param>
        internal ElasticConnection(HttpMessageHandler innerMessageHandler, Uri endpoint, string userName = null, string password = null, string index = null, TimeSpan? timeout = null, ElasticConnectionOptions options = null)
            : base(index, timeout, options)
        {
            Argument.EnsureNotNull(nameof(endpoint), endpoint);

            this.Endpoint = endpoint;

            var httpClientHandler = innerMessageHandler as HttpClientHandler;
            if (httpClientHandler != null && httpClientHandler.SupportsAutomaticDecompression)
                httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip;

            HttpClient = new HttpClient(new ForcedAuthHandler(userName, password, innerMessageHandler), true);
        }

        /// <summary>
        /// The HttpClient used for issuing HTTP network requests.
        /// </summary>
        internal HttpClient HttpClient { get; private set; }

        /// <summary>
        /// The Uri that specifies the public endpoint for the server.
        /// </summary>
        /// <example>http://myserver.example.com:9200</example>
        public Uri Endpoint { get; }

        /// <summary>
        /// Dispose of this ElasticConnection and any associated resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (HttpClient != null)
                {
                    HttpClient.Dispose();
                    HttpClient = null;
                }
            }
        }

        /// <inheritdoc/>
        public override async Task<ElasticResponse> SearchAsync(
            string body,
            SearchRequest searchRequest,
            CancellationToken token,
            ILog log)
        {
            var uri = GetSearchUri(searchRequest);

            log.Debug(null, null, "Request: POST {0}", uri);
            log.Debug(null, null, "Body:\n{0}", body);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri) { Content = new StringContent(body) })
            {
                requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                try
                {
                    using (var response = await SendRequestAsync(requestMessage, token, log).ConfigureAwait(false))
                    {
                        using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                            return ParseResponse(responseStream, log);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is HttpRequestException)
                    {
                    }

                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public override Uri GetSearchUri(SearchRequest searchRequest)
        {
            var builder = new UriBuilder(Endpoint);
            var index = string.IsNullOrEmpty(Index)?searchRequest.IndexType : Index;
            builder.Path += string.IsNullOrEmpty(index) ? "*/": $"{index}/";

            builder.Path += "_search";

            var parameters = builder.Uri.GetComponents(UriComponents.Query, UriFormat.Unescaped)
                .Split(parameterSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split('='))
                .ToDictionary(k => k[0], v => v.Length > 1 ? v[1] : null);

            if (Options.Pretty)
                parameters["pretty"] = "true";

            builder.Query = string.Join("&", parameters.Select(p => p.Value == null ? p.Key : p.Key + "=" + p.Value));

            return builder.Uri;
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage requestMessage, CancellationToken token, ILog log)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await HttpClient.SendAsync(requestMessage, token).ConfigureAwait(false);
            stopwatch.Stop();

            log.Debug(null, null, "Response: {0} {1} (in {2}ms)", (int)response.StatusCode, response.StatusCode, stopwatch.ElapsedMilliseconds);

            response.EnsureSuccessStatusCode();

            return response;
        }

        internal static ElasticResponse ParseResponse(Stream responseStream, ILog log)
        {
            var stopwatch = Stopwatch.StartNew();

            using (var textReader = new JsonTextReader(new StreamReader(responseStream)))
            {
                var results = new JsonSerializer().Deserialize<ElasticResponse>(textReader);
                stopwatch.Stop();

                var resultSummary = String.Join(", ", GetResultSummary(results));
                log.Debug(null, null, "Deserialized {0} bytes into {1} in {2}ms", responseStream.Length, resultSummary, stopwatch.ElapsedMilliseconds);

                return results;
            }
        }

        internal static IEnumerable<string> GetResultSummary(ElasticResponse results)
        {
            if (results == null)
            {
                yield return "nothing";
            }
            else
            {
                if (results.hits?.hits != null && results.hits.hits.Count > 0)
                    yield return results.hits.hits.Count + " hits";
            }
        }

        /// <inheritdoc/>
        public override async Task<Dictionary<string, string>> GetPropertiesMappings(ILog log, CancellationToken token = default(CancellationToken))
        {
            var uri = GetMappingUri();
            var propertMappings = new Dictionary<string, string>();
            log.Debug(null, null, "Request: Get {0}", uri);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                try
                {
                    using (var response = await SendRequestAsync(requestMessage, token, log).ConfigureAwait(false))
                    {
                        using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            using (var textReader = new JsonTextReader(new StreamReader(responseStream)))
                            {
                                JObject responseBody = new JsonSerializer().Deserialize<JObject>(textReader);
                                if (responseBody != null) ParseMappingResponse(responseBody.First, propertMappings);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex is HttpRequestException)
                    {
                    }

                    throw;
                }
            }
            return propertMappings;
        }

        private Uri GetMappingUri()
        {
            var builder = new UriBuilder(Endpoint);
            builder.Path += (Index ?? "*") + "/";

            builder.Path += "_mapping";

            return builder.Uri;
        }

        internal static Dictionary<string, string> ParseMappingResponse(JToken index, Dictionary<string, string> propertMappings)
        {
            if (index != null)
            {
                if (index.First != null)
                {
                    var propertiesObject = index.First["mappings"]?["properties"];
                    FetchProperties(propertiesObject, propertMappings);
                }

                return ParseMappingResponse(index.Next, propertMappings);
            }
            else
            {
                return propertMappings;
            }
        }

        internal static void FetchProperties(JToken propertiesObject, Dictionary<string, string> propertMappings, string parentProperty= "")
        {
            if (propertiesObject != null)
            {
                var property = propertiesObject.First;
                while (property != null)
                {
                    var propertyName = ((JProperty)property).Name;
                    var key =string.IsNullOrEmpty(parentProperty)? propertyName : $"{parentProperty}.{propertyName}";
                    if (!propertMappings.ContainsKey(key))
                    {
                        string val;
                        var type = property.First["type"];
                        if (type != null)
                        {
                            val = ((JValue)type).Value.ToString();
                            if (!string.IsNullOrEmpty(val))
                            {
                                propertMappings.Add(key, val);
                            }
                        }
                        var fieldtype = property.First["fields"]?["keyword"]?["type"];
                        if (fieldtype != null)
                        {
                            val = ((JValue)fieldtype).Value.ToString();
                            propertMappings.Add($"{key}.keyword", val);
                        }
                        var properties = property.First["properties"];
                        if (properties != null)
                        {
                            FetchProperties(properties,propertMappings,key);
                        }
                    }
                    property = property.Next;
                }
            }
        }
    }
}