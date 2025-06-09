using System;
using System.Collections.Generic;

namespace Core
{
    /// <summary>  
    /// Builder for creating a collection of HTTP headers with fluent API  
    /// </summary>  
    public class HttpHeaderBuilder
    {
        private readonly List<HttpRequestHeader> _headers = new List<HttpRequestHeader>();

        /// <summary>  
        /// Creates a new builder instance  
        /// </summary>  
        public static HttpHeaderBuilder Create()
        {
            return new HttpHeaderBuilder();
        }

        /// <summary>  
        /// Creates a new builder with default headers  
        /// </summary>  
        public static HttpHeaderBuilder CreateWithDefaults()
        {
            return new HttpHeaderBuilder().WithDefaults();
        }

        /// <summary>  
        /// Adds default headers  
        /// </summary>  
        public HttpHeaderBuilder WithDefaults()
        {
            _headers.AddRange(HttpRequestHeader.Defaults());
            return this;
        }

        /// <summary>  
        /// Adds a custom header  
        /// </summary>  
        public HttpHeaderBuilder WithHeader(string name, string value)
        {
            _headers.Add(HttpRequestHeader.Create(name, value));
            return this;
        }

        /// <summary>  
        /// Adds an existing HttpRequestHeader  
        /// </summary>  
        public HttpHeaderBuilder WithHeader(HttpRequestHeader header)
        {
            _headers.Add(header);
            return this;
        }

        /// <summary>  
        /// Adds multiple headers  
        /// </summary>  
        public HttpHeaderBuilder WithHeaders(IEnumerable<HttpRequestHeader> headers)
        {
            _headers.AddRange(headers);
            return this;
        }

        /// <summary>  
        /// Adds JSON content type header  
        /// </summary>  
        public HttpHeaderBuilder AsJson()
        {
            _headers.Add(HttpRequestHeader.JsonContent());
            return this;
        }

        /// <summary>  
        /// Adds text content type header  
        /// </summary>  
        public HttpHeaderBuilder AsText()
        {
            _headers.Add(HttpRequestHeader.TextContent());
            return this;
        }

        /// <summary>  
        /// Adds binary content type header  
        /// </summary>  
        public HttpHeaderBuilder AsBinary()
        {
            _headers.Add(HttpRequestHeader.BinaryContent());
            return this;
        }

        /// <summary>  
        /// Adds Bearer token authorization  
        /// </summary>  
        public HttpHeaderBuilder WithBearerToken(string token)
        {
            _headers.Add(HttpRequestHeader.BearerAuth(token));
            return this;
        }

        /// <summary>  
        /// Adds Basic authentication  
        /// </summary>  
        public HttpHeaderBuilder WithBasicAuth(string username, string password)
        {
            _headers.Add(HttpRequestHeader.BasicAuth(username, password));
            return this;
        }

        /// <summary>  
        /// Adds headers to disable caching  
        /// </summary>  
        public HttpHeaderBuilder WithCachingDisabled()
        {
            _headers.AddRange(HttpRequestHeader.DisableCache());
            return this;
        }

        /// <summary>  
        /// Gets all headers as a list  
        /// </summary>  
        public IList<HttpRequestHeader> Build()
        {
            return new List<HttpRequestHeader>(_headers);
        }

        /// <summary>  
        /// Converts headers to a dictionary  
        /// </summary>  
        public Dictionary<string, string> BuildDictionary()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var header in _headers)
            {
                result[header.Name] = header.Value;
            }
            return result;
        }
    }
}
