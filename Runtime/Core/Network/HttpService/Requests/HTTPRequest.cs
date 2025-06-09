using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Text;
using System.Diagnostics;

namespace Core
{
    /// <summary>  
    /// Handles HTTP requests with improved timeout and cancellation handling  
    /// </summary>  
    public class HTTPRequest
    {
        #region Private Fields

        private readonly string _url;
        private readonly HTTPMethods _httpMethod;
        private readonly HttpRequestOptions _options;
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
        private string _body;
        private string _contentType;
        private Action<float> _onProgress;
        private bool _disableCache;
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private const string LOG_TAG = "[HTTPRequest]";
        private bool _useExponentialBackoff;

        // Streaming fields
        private Action<byte[]> _onChunkReceived;
        private Action<long, long> _onProgressChanged;
        private Action _onCompleted;
        private Action<string> _onChunkError;
        private List<byte> _accumulatedData;
        private bool _enableStreaming = false;

        // Performance tracking
        private readonly Stopwatch _requestStopwatch = new Stopwatch();

        #endregion

        /// <summary>
        /// Gets current accumulated data size
        /// </summary>
        public int AccumulatedDataSize => _accumulatedData.Count;

        #region Constructor

        /// <summary>  
        /// Creates a new HTTP request  
        /// </summary>  
        public HTTPRequest(string url, HTTPMethods httpMethod, HttpRequestOptions options)
        {
            _url = url ?? throw new ArgumentNullException(nameof(url));
            _httpMethod = httpMethod;
            _options = options?.Clone() ?? throw new ArgumentNullException(nameof(options));
            _options.Validate();
            _disableCache = options.DisableCache;
            _useExponentialBackoff = false;
        }

        #endregion

        #region Public Configuration Methods

        /// <summary>  
        /// Sets a header for the request  
        /// </summary>  
        public HTTPRequest SetHeader(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Header key cannot be null or empty", nameof(key));

            _headers[key] = value;
            return this;
        }

        /// <summary>
        /// Sets multiple headers at once
        /// </summary>
        public HTTPRequest SetHeaders(Dictionary<string, string> headers)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    _headers[header.Key] = header.Value;
                }
            }
            return this;
        }

        /// <summary>
        /// Removes a header
        /// </summary>
        public HTTPRequest RemoveHeader(string key)
        {
            _headers.Remove(key);
            return this;
        }

        /// <summary>  
        /// Sets the request body and content type  
        /// </summary>  
        public HTTPRequest SetBody(string body, string contentType = "application/json")
        {
            _body = body;
            _contentType = contentType;
            return this;
        }

        /// <summary>
        /// Sets JSON body
        /// </summary>
        public HTTPRequest SetJsonBody(object obj)
        {
            if (obj != null)
            {
                _body = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                _contentType = "application/json";
            }
            return this;
        }

        /// <summary>  
        /// Disables cache for this request  
        /// </summary>  
        public HTTPRequest DisableCache()
        {
            _disableCache = true;
            return this;
        }

        /// <summary>  
        /// Enables exponential backoff for retry delays  
        /// </summary>  
        public HTTPRequest WithExponentialBackoff(bool enable = true)
        {
            _useExponentialBackoff = enable;
            return this;
        }

        /// <summary>  
        /// Sets a callback for download progress  
        /// </summary>  
        public HTTPRequest OnDownloadProgress(Action<float> onProgress)
        {
            _onProgress = onProgress;
            return this;
        }

        /// <summary>
        /// Enables streaming mode with chunk callbacks
        /// </summary>
        public HTTPRequest EnableStreaming(
            Action<byte[]> onChunkReceived = null,
            Action<long, long> onProgressChanged = null,
            Action onCompleted = null)
        {
            _enableStreaming = true;
            _onChunkReceived = onChunkReceived;
            _onProgressChanged = onProgressChanged;
            _onCompleted = onCompleted;
            return this;
        }

        /// <summary>
        /// Sets authentication header
        /// </summary>
        public HTTPRequest SetAuth(string token, string scheme = "Bearer")
        {
            if (!string.IsNullOrEmpty(token))
            {
                SetHeader("Authorization", $"{scheme} {token}");
            }
            return this;
        }

        /// <summary>
        /// Sets basic authentication
        /// </summary>
        public HTTPRequest SetBasicAuth(string username, string password)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                SetHeader("Authorization", $"Basic {credentials}");
            }
            return this;
        }

        #endregion

        #region Private Request Creation

        /// <summary>  
        /// Creates UnityWebRequest based on method and body  
        /// </summary>  
        private UnityWebRequest CreateUnityWebRequest()
        {
            UnityWebRequest request;

            switch (_httpMethod)
            {
                case HTTPMethods.Get:
                    request = UnityWebRequest.Get(_url);
                    break;

                case HTTPMethods.Post:
                    request = new UnityWebRequest(_url, UnityWebRequest.kHttpVerbPOST);
                    if (!string.IsNullOrEmpty(_body))
                    {
                        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(_body));
                        request.uploadHandler.contentType = _contentType;
                    }
                    break;

                case HTTPMethods.Put:
                    request = UnityWebRequest.Put(_url, _body);
                    if (!string.IsNullOrEmpty(_contentType))
                    {
                        request.SetRequestHeader("Content-Type", _contentType);
                    }
                    break;

                case HTTPMethods.Patch:
                    request = new UnityWebRequest(_url, "PATCH");
                    if (!string.IsNullOrEmpty(_body))
                    {
                        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(_body));
                        request.uploadHandler.contentType = _contentType;
                    }
                    break;

                case HTTPMethods.Delete:
                    request = UnityWebRequest.Delete(_url);
                    break;

                case HTTPMethods.Head:
                    request = UnityWebRequest.Head(_url);
                    break;

                default:
                    throw new NotSupportedException($"HTTP method {_httpMethod} is not supported");
            }

            ApplyCacheControl(request);

            // Set download handler based on streaming mode
            if (_enableStreaming)
            {
                _accumulatedData = new List<byte>();
                request.downloadHandler = new ChunkedDownloadHandler(
                    onChunkReceived: OnChunk,
                    onProgressChanged: OnProgress,
                    onCompleted: OnCompleted,
                    onError: OnError,
                    accumulate: false // Don't store in memory for streaming
                );
            }
            else
            {
                // Use default download handler for normal requests
                request.downloadHandler = new DownloadHandlerBuffer();
            }

            return request;
        }

        private void ApplyCacheControl(UnityWebRequest request)
        {
            if (!_disableCache)
                return;

            // Disable caching using standard HTTP headers  
            request.SetRequestHeader("Cache-Control", "no-cache, no-store, must-revalidate");
            request.SetRequestHeader("Pragma", "no-cache");
            request.SetRequestHeader("Expires", "0");

            // Add a unique parameter to ensure no caching  
            _stringBuilder.Clear();
            var separator = request.url.Contains("?") ? "&" : "?";
            _stringBuilder.Append(request.url).Append(separator).Append("_nocache=").Append(Guid.NewGuid().ToString());
            request.url = _stringBuilder.ToString();
        }

        private void ApplyRequestHeaders(UnityWebRequest request)
        {
            // Apply custom headers  
            foreach (var kv in _headers)
            {
                try
                {
                    request.SetRequestHeader(kv.Key, kv.Value);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"{LOG_TAG} Failed to set header '{kv.Key}': {ex.Message}");
                }
            }

            // Apply user agent
            if (!string.IsNullOrWhiteSpace(_options.UserAgent))
            {
                try
                {
                    request.SetRequestHeader("User-Agent", _options.UserAgent);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"{LOG_TAG} Failed to set User-Agent: {ex.Message}");
                }
            }

            // Apply options headers
            foreach (var customHeader in _options.CustomHeaders)
            {
                try
                {
                    request.SetRequestHeader(customHeader.Key, customHeader.Value);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"{LOG_TAG} Failed to set custom header '{customHeader.Key}': {ex.Message}");
                }
            }
        }

        #endregion

        #region Streaming Callbacks

        private void OnChunk(byte[] chunk)
        {
            try
            {
                _accumulatedData.AddRange(chunk);
                _onChunkReceived?.Invoke(chunk);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_TAG} Error processing chunk for {_url}: {ex.Message}");
            }
        }

        private void OnProgress(long received, long total)
        {
            try
            {
                float progressPercent = total > 0 ? (float) received / total : 0f;
                _onProgress?.Invoke(progressPercent);
                _onProgressChanged?.Invoke(received, total);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_TAG} Error processing progress for {_url}: {ex.Message}");
            }
        }

        private void OnCompleted()
        {
            try
            {
                _onCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_TAG} Error processing completion for {_url}: {ex.Message}");
            }
        }


        private void OnError(string obj)
        {
            _onChunkError?.Invoke(obj);
        }

        #endregion

        #region Public Data Access

        /// <summary>
        /// Gets accumulated data from streaming
        /// </summary>
        public byte[] GetAccumulatedData()
        {
            return _accumulatedData.ToArray();
        }

        /// <summary>
        /// Gets accumulated text from streaming
        /// </summary>
        public string GetAccumulatedText()
        {
            return _accumulatedData.Count > 0 ? Encoding.UTF8.GetString(_accumulatedData.ToArray()) : null;
        }

        /// <summary>
        /// Clears accumulated data to free memory
        /// </summary>
        public void ClearAccumulatedData()
        {
            _accumulatedData.Clear();
        }

        #endregion

        #region Main Send Method

        /// <summary>  
        /// Sends the request with improved timeout and retry handling  
        /// </summary>  
        public async UniTask<HTTPResponse> SendAsync(CancellationToken cancellationToken = default)
        {
            if (!Application.isPlaying)
            {
                return CreateErrorResponse("Application is not in play mode");
            }

            // Start performance tracking
            _requestStopwatch.Restart();

            // Create cancellation tokens for different timeout scenarios  
            using var userCancellationCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, _options.CancellationToken);

            using var totalTimeoutCts = new CancellationTokenSource(_options.Timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                userCancellationCts.Token, totalTimeoutCts.Token);

            UnityWebRequest request = null;
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                int attempts = 0;
                HTTPResponse response = null;
                Exception lastError = null;

                while (attempts <= _options.MaxRetries && Application.isPlaying)
                {
                    request?.Dispose();
                    request = null;

                    if (linkedCts.Token.IsCancellationRequested)
                    {
                        string cancelSource = totalTimeoutCts.IsCancellationRequested ? "Total timeout" : "User cancellation";
                        break;
                    }

                    attempts++;
                    lastError = null;

                    // Calculate remaining time for this attempt  
                    TimeSpan elapsedTime = stopwatch.Elapsed;
                    TimeSpan remainingTime = _options.Timeout - elapsedTime;
                    if (remainingTime <= TimeSpan.Zero)
                    {
                        UnityEngine.Debug.LogWarning($"{LOG_TAG} No time left for request to {_url}. Total timeout reached.");
                        totalTimeoutCts.Cancel();
                        break;
                    }

                    // Create connection timeout for this attempt  
                    var connectionTimeout = TimeSpan.FromMilliseconds(
                        Math.Min(_options.ConnectionTimeout.TotalMilliseconds, remainingTime.TotalMilliseconds)
                    );

                    using var connectionCts = new CancellationTokenSource(connectionTimeout);
                    using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(linkedCts.Token, connectionCts.Token);

                    try
                    {
                        request = CreateUnityWebRequest();
                        ApplyRequestHeaders(request);

                        // Set timeout to the smaller of connection timeout or remaining time  
                        request.timeout = (int)Math.Min(_options.ConnectionTimeout.TotalSeconds, remainingTime.TotalSeconds);

                        // Send the request  
                        var operation = request.SendWebRequest();
                        response = await ProcessRequestOperation(operation, request, attemptCts.Token);

                        if (!Application.isPlaying)
                            break;

                        // Process response outcome  
                        if (response.IsSuccess)
                        {
                            break; // Success, no need to retry  
                        }
                        else if (ShouldRetry(response))
                        {
                            // Server error, timeout or network issue - retry if we have tries left  
                            lastError = new HttpException(response.StatusCode, response.Error);
                            UnityEngine.Debug.LogWarning($"{LOG_TAG} Retriable error for {_url}: StatusCode={response.StatusCode}, Error={response.Error}");
                        }
                        else
                        {
                            // Other errors - don't retry  
                            UnityEngine.Debug.LogWarning($"{LOG_TAG} Non-retriable error for {_url}: StatusCode={response.StatusCode}, Error={response.Error}");
                            break;
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        lastError = ex;
                        UnityEngine.Debug.LogWarning($"{LOG_TAG} Connection timeout for {_url}: {ex.Message}");
                        // Connection timeout - retry  
                    }
                    catch (OperationCanceledException ex)
                    {
                        lastError = ex;
                        response = HandleCancellation(ex, totalTimeoutCts, userCancellationCts, stopwatch);
                        if (response != null) break; // Don't retry if it was a deliberate cancellation  
                    }
                    catch (Exception ex)
                    {
                        lastError = ex;
                        UnityEngine.Debug.LogError($"{LOG_TAG} Error for {_url}: {ex.GetType().Name}: {ex.Message}");

                        // Retry for network-related errors or if response couldn't be processed  
                        if ((ex.Message.Contains("network") || ex is NullReferenceException) && attempts <= _options.MaxRetries)
                        {
                            UnityEngine.Debug.Log($"{LOG_TAG} Will retry after network error");
                        }
                        else
                        {
                            response = CreateErrorResponse($"Request failed: {ex.Message}");
                            break;
                        }
                    }
                    finally
                    {
                        if (attempts <= _options.MaxRetries && Application.isPlaying && lastError != null)
                        {
                            request?.Dispose();
                            request = null;

                            await DelayBeforeRetry(linkedCts.Token, attempts);
                        }
                    }
                }

                if (!Application.isPlaying)
                {
                    return CreateErrorResponse("Application stopped", isCanceled: true);
                }

                if (linkedCts.Token.IsCancellationRequested)
                {
                    string cancelReason = totalTimeoutCts.IsCancellationRequested
                                                            ? $"Total timeout after {stopwatch.Elapsed.TotalSeconds:F1}s"
                                                            : "User cancellation";

                    return CreateErrorResponse(cancelReason, isCanceled: true);
                }

                // Add performance data to response
                if (response != null)
                {
                    response.ResponseTimeMs = _requestStopwatch.ElapsedMilliseconds;
                }

                // Return last response or error  
                return response ?? CreateErrorResponse(lastError?.Message ?? "Unknown error");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_TAG} Unexpected error for {_url}: {ex.Message}");
                return CreateErrorResponse($"Unexpected error: {ex.Message}");
            }
            finally
            {
                stopwatch.Stop();
                _requestStopwatch.Stop();
                request?.Dispose();
            }
        }

        #endregion

        #region Private Helper Methods

        private async UniTask<HTTPResponse> ProcessRequestOperation(
            UnityWebRequestAsyncOperation operation,
            UnityWebRequest request,
            CancellationToken token)
        {
            float lastProgress = 0;

            while (!operation.isDone)
            {
                // Handle progress for non-streaming requests
                if (!_enableStreaming && _onProgress != null && request.downloadProgress >= 0 &&
                    Math.Abs(lastProgress - request.downloadProgress) > 0.01f)
                {
                    lastProgress = request.downloadProgress;
                    _onProgress(lastProgress);
                }

                await UniTask.Yield(token);

                if (!Application.isPlaying)
                {
                    throw new OperationCanceledException("Application stopped playing");
                }
            }

            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException("Request was canceled", token);
            }

            if (request.downloadHandler == null)
            {
                UnityEngine.Debug.LogError($"{LOG_TAG} Download handler is null for {_url}");
                throw new NullReferenceException("Download handler is null");
            }

            // Process response based on streaming mode
            byte[] responseData;
            string responseText;

            if (_enableStreaming)
            {
                // Use accumulated data from streaming
                responseData = _accumulatedData.Count > 0 ? _accumulatedData.ToArray() : null;
                responseText = responseData != null ? Encoding.UTF8.GetString(responseData) : null;
            }
            else
            {
                // Use standard download handler data
                responseData = request.downloadHandler.data;
                responseText = request.downloadHandler.text;
            }

            return new HTTPResponse
            {
                IsSuccess = request.result == UnityWebRequest.Result.Success,
                StatusCode = (int)request.responseCode,
                StatusMessage = HTTPResponse.GetStatusMessage((int)request.responseCode),
                Data = responseData,
                Text = responseText,
                Error = request.error,
                Headers = request.GetResponseHeaders() ?? new Dictionary<string, string>(),
                IsCanceled = false,
                ResponseTimeMs = _requestStopwatch.ElapsedMilliseconds
            };
        }

        private bool ShouldRetry(HTTPResponse response)
        {
            return response.StatusCode >= 500 ||
                   response.StatusCode == 408 || // Request Timeout
                   response.StatusCode == 429 || // Too Many Requests
                   response.Error?.ToLower().Contains("timeout") == true ||
                   response.Error?.ToLower().Contains("network") == true ||
                   response.Error?.ToLower().Contains("connection") == true;
        }

        private HTTPResponse HandleCancellation(OperationCanceledException ex,
                                                CancellationTokenSource totalTimeoutCts,
                                                CancellationTokenSource userCancellationCts,
                                                Stopwatch stopwatch)
        {
            if (totalTimeoutCts.IsCancellationRequested)
            {
                UnityEngine.Debug.LogWarning($"{LOG_TAG} Total timeout for {_url} after {stopwatch.Elapsed.TotalSeconds:F1}s");
                return CreateErrorResponse($"Total timeout after {stopwatch.Elapsed.TotalSeconds:F1}s", isCanceled: true);
            }
            else if (userCancellationCts.IsCancellationRequested)
            {
                UnityEngine.Debug.LogWarning($"{LOG_TAG} User canceled request to {_url}");
                return CreateErrorResponse("User canceled request", isCanceled: true);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"{LOG_TAG} Connection timeout for {_url}");
                return null; // Connection timeout - we can retry  
            }
        }

        private async UniTask DelayBeforeRetry(CancellationToken token, int attemptNumber)
        {
            int delay = _options.RetryDelayMilliseconds;
            if (_useExponentialBackoff)
            {
                delay = Math.Min(delay * (int)Math.Pow(2, attemptNumber - 1), 30000); // Cap at 30 seconds  
            }

            try
            {
                await UniTask.Delay(delay, cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        private HTTPResponse CreateErrorResponse(string error, bool isCanceled = false)
        {
            return new HTTPResponse
            {
                StatusCode = 0,
                Error = error,
                IsSuccess = false,
                IsCanceled = isCanceled,
                ResponseTimeMs = _requestStopwatch.ElapsedMilliseconds
            };
        }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// Creates a GET request
        /// </summary>
        public static HTTPRequest Get(string url, HttpRequestOptions options = null)
        {
            return new HTTPRequest(url, HTTPMethods.Get, options ?? new HttpRequestOptions());
        }

        /// <summary>
        /// Creates a POST request
        /// </summary>
        public static HTTPRequest Post(string url, object data = null, HttpRequestOptions options = null)
        {
            var request = new HTTPRequest(url, HTTPMethods.Post, options ?? new HttpRequestOptions());
            if (data != null)
            {
                request.SetJsonBody(data);
            }
            return request;
        }

        /// <summary>
        /// Creates a PUT request
        /// </summary>
        public static HTTPRequest Put(string url, object data = null, HttpRequestOptions options = null)
        {
            var request = new HTTPRequest(url, HTTPMethods.Put, options ?? new HttpRequestOptions());
            if (data != null)
            {
                request.SetJsonBody(data);
            }
            return request;
        }

        /// <summary>
        /// Creates a DELETE request
        /// </summary>
        public static HTTPRequest Delete(string url, HttpRequestOptions options = null)
        {
            return new HTTPRequest(url, HTTPMethods.Delete, options ?? new HttpRequestOptions());
        }

        #endregion
    }
}