using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Abstract base class for mapping and handling HTTP responses in a strongly-typed way.
    /// Inherit from this to represent a specific API response.
    /// </summary>
    public abstract class ClientHttpResponseBase
    {
        /// <summary>
        /// Indicates if the HTTP request succeeded (according to HTTPResponse.IsSuccess).
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// HTTP status code of the response (e.g. 200, 404).
        /// </summary>
        public long StatusCode { get; set; }

        /// <summary>
        /// Status message, e.g. "OK" or "Not Found".
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// Response headers.
        /// </summary>
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// The raw text body of the HTTP response, if available.
        /// </summary>
        public string RawText { get; set; }

        public byte[] Data { get; set; }

        /// <summary>
        /// HTTP error string in case of request/network failure, else null.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Indicates if the response was served from cache.
        /// </summary>
        public bool IsCached { get; set; }

        /// <summary>
        /// Standard method to map the HTTPResponse into this object.
        /// </summary>
        public virtual void FromHttpResponse(HTTPResponse response)
        {
            if (response != null)
            {
                IsSuccess = response.IsSuccess;
                StatusCode = response.StatusCode;
                StatusMessage = response.StatusMessage;
                Headers = new Dictionary<string, string>(response.Headers ?? new Dictionary<string, string>());
                RawText = response.Text;
                ErrorMessage = response.Error;
                IsCached = response.IsCached;

                if (IsSuccess)
                {
                    OnHandleResponse(response);
                }
            }
            else
            {
                // Handle null response (timeout or severe connection issue)
                IsSuccess = false;
                StatusCode = 0;
                StatusMessage = "No response (timeout or connection failure)";
                ErrorMessage = "Request timed out or no response received";
                IsCached = false;
            }
        }

        /// <summary>
        /// Subclasses implement this method to parse the body or process the response further.
        /// Only called when IsSuccess == true.
        /// </summary>
        public abstract void OnHandleResponse(HTTPResponse response);
    }
}

