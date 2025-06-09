using System;
using System.Collections.Generic;
using System.Threading;

namespace Core
{
    /// <summary>
    /// Encapsulates HTTP request configuration options with enhanced cache support.
    /// </summary>
    public class HttpRequestOptions
    {
        /// <summary>
        /// Total request timeout (includes connection and data transfer).
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Timeout specifically for establishing the connection.
        /// </summary>
        public TimeSpan ConnectionTimeout { get; set; }

        /// <summary>
        /// How many times to retry the request if it fails.
        /// </summary>
        public int MaxRetries { get; set; }

        /// <summary>
        /// Milliseconds to wait between retry attempts. Zero means no delay.
        /// </summary>
        public int RetryDelayMilliseconds { get; set; }

        /// <summary>
        /// If true, enables timeout enforcement for streaming operations.
        /// </summary>
        public bool EnableTimeoutForStreaming { get; set; }

        /// <summary>
        /// If true, disables caching by adding cache-busting parameters.
        /// </summary>
        public bool DisableCache { get; set; }

        /// <summary>
        /// Optional additional headers to include with requests.
        /// </summary>
        public Dictionary<string, string> CustomHeaders { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Optional User-Agent string to use for the request.
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Optional token to support request cancellation.
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        private static readonly object _lock = new object();
        private static HttpRequestOptions _default;

        /// <summary>
        /// Gets the default request options. Thread-safe.
        /// </summary>
        public static HttpRequestOptions Default
        {
            get
            {
                lock (_lock)
                {
                    if (_default == null)
                        _default = new HttpRequestOptions();
                    return _default;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance with default values.
        /// </summary>
        public HttpRequestOptions()
        {
            Timeout = TimeSpan.FromSeconds(60);
            ConnectionTimeout = TimeSpan.FromSeconds(50);
            MaxRetries = 3;
            RetryDelayMilliseconds = 1000;
            EnableTimeoutForStreaming = false;
            DisableCache = true;
            UserAgent = "HttpRequestOptions/1.0";
        }

        /// <summary>
        /// Creates a shallow copy of the current options.
        /// </summary>
        public HttpRequestOptions Clone()
        {
            return new HttpRequestOptions
            {
                Timeout = Timeout,
                ConnectionTimeout = ConnectionTimeout,
                MaxRetries = MaxRetries,
                RetryDelayMilliseconds = RetryDelayMilliseconds,
                EnableTimeoutForStreaming = EnableTimeoutForStreaming,
                DisableCache = DisableCache,
                CustomHeaders = new Dictionary<string, string>(CustomHeaders),
                UserAgent = UserAgent,
                CancellationToken = CancellationToken
            };
        }

        /// <summary>
        /// Validates the options and throws exceptions if invalid values are detected.
        /// </summary>
        public void Validate()
        {
            if (Timeout <= TimeSpan.Zero)
                throw new ArgumentException("Timeout must be greater than zero");
            if (ConnectionTimeout <= TimeSpan.Zero)
                throw new ArgumentException("ConnectionTimeout must be greater than zero");
            if (MaxRetries < 0)
                throw new ArgumentException("MaxRetries cannot be negative");
            if (RetryDelayMilliseconds < 0)
                throw new ArgumentException("RetryDelayMilliseconds cannot be negative");
        }

        #region Builder Methods

        public HttpRequestOptions WithTimeout(TimeSpan timeout)
        {
            Timeout = timeout;
            return this;
        }

        public HttpRequestOptions WithConnectionTimeout(TimeSpan timeout)
        {
            ConnectionTimeout = timeout;
            return this;
        }

        public HttpRequestOptions WithMaxRetries(int retries)
        {
            MaxRetries = retries;
            return this;
        }

        public HttpRequestOptions WithRetryDelay(int milliseconds)
        {
            RetryDelayMilliseconds = milliseconds;
            return this;
        }

        public HttpRequestOptions WithStreamingTimeout(bool enable)
        {
            EnableTimeoutForStreaming = enable;
            return this;
        }

        public HttpRequestOptions WithCaching(bool enableCaching)
        {
            DisableCache = !enableCaching;
            return this;
        }

        /// <summary>  
        /// Disable caching for this request  
        /// </summary>  
        public HttpRequestOptions WithCachingDisabled(bool disable = true)
        {
            DisableCache = disable;
            return this;
        }

        public HttpRequestOptions WithUserAgent(string userAgent)
        {
            UserAgent = userAgent;
            return this;
        }

        public HttpRequestOptions WithHeader(string name, string value)
        {
            CustomHeaders[name] = value;
            return this;
        }

        public HttpRequestOptions WithCancellationToken(CancellationToken token)
        {
            CancellationToken = token;
            return this;
        }

        #endregion

        /// <summary>
        /// Creates a new instance optimized for downloading large files with caching.
        /// </summary>
        public static HttpRequestOptions CreateForLargeDownload()
        {
            return new HttpRequestOptions()
                .WithTimeout(TimeSpan.FromMinutes(10))
                .WithConnectionTimeout(TimeSpan.FromMinutes(3))
                .WithMaxRetries(3)
                .WithRetryDelay(5000)
                .WithStreamingTimeout(true)
                .WithCaching(true); // Enable caching
        }

        /// <summary>
        /// Creates a new instance optimized for quick API calls.
        /// </summary>
        public static HttpRequestOptions CreateForApi()
        {
            return new HttpRequestOptions()
                .WithTimeout(TimeSpan.FromSeconds(30))
                .WithConnectionTimeout(TimeSpan.FromSeconds(10))
                .WithMaxRetries(3)
                .WithRetryDelay(500)
                .WithCaching(false); // Disable caching for APIs
        }
    }
}

