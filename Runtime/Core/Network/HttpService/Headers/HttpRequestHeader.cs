using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>  
    /// Represents an HTTP request header with fluent API support  
    /// </summary>  
    public struct HttpRequestHeader
    {
        /// <summary>  
        /// Header name  
        /// </summary>  
        public string Name { get; }

        /// <summary>  
        /// Header value  
        /// </summary>  
        public string Value { get; }

        public HttpRequestHeader(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>  
        /// Create a new header with specified name and value  
        /// </summary>  
        public static HttpRequestHeader Create(string name, string value)
        {
            return new HttpRequestHeader(name, value);
        }

        /// <summary>  
        /// Get a collection of common default headers  
        /// </summary>  
        public static IList<HttpRequestHeader> Defaults()
        {
            return new List<HttpRequestHeader>
            {
                Create("Accept-Language", "en-US,en;q=0.9"),
                Create("User-Agent", "UnityClient/1.0")
            };
        }

        /// <summary>  
        /// Creates a Content-Type header for text/plain  
        /// </summary>  
        public static HttpRequestHeader TextContent()
        {
            return Create("Content-Type", "text/plain");
        }

        /// <summary>  
        /// Creates a Content-Type header for application/json  
        /// </summary>  
        public static HttpRequestHeader JsonContent()
        {
            return Create("Content-Type", "application/json; charset=UTF-8");
        }

        /// <summary>  
        /// Creates a Content-Type header for application/octet-stream  
        /// </summary>  
        public static HttpRequestHeader BinaryContent()
        {
            return Create("Content-Type", "application/octet-stream");
        }

        /// <summary>  
        /// Creates an Accept header for application/pdf  
        /// </summary>  
        public static HttpRequestHeader AcceptPdf()
        {
            return Create("Accept", "application/pdf");
        }

        /// <summary>  
        /// Creates an Accept header for MS Word documents  
        /// </summary>  
        public static HttpRequestHeader AcceptWord()
        {
            return Create("Accept", "application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        }

        /// <summary>  
        /// Creates an Accept header for ZIP files  
        /// </summary>  
        public static HttpRequestHeader AcceptZip()
        {
            return Create("Accept", "application/zip");
        }

        /// <summary>  
        /// Creates an Authorization header with Bearer token  
        /// </summary>  
        public static HttpRequestHeader BearerAuth(string token)
        {
            return Create("Authorization", "Bearer " + token);
        }

        /// <summary>  
        /// Creates an Authorization header with Basic authentication  
        /// </summary>  
        public static HttpRequestHeader BasicAuth(string username, string password)
        {
            string credentials = System.Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
            return Create("Authorization", "Basic " + credentials);
        }

        /// <summary>  
        /// Creates a header to disable caching  
        /// </summary>  
        public static IList<HttpRequestHeader> DisableCache()
        {
            return new List<HttpRequestHeader>
            {
                Create("Cache-Control", "no-cache, no-store, must-revalidate"),
                Create("Pragma", "no-cache"),
                Create("Expires", "0")
            };
        }
    }
}