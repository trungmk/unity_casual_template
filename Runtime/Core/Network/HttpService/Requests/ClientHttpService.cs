using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;

namespace Core
{
    /// <summary>
    /// High-level HTTP client service with strongly-typed responses and advanced features
    /// </summary>
    public class ClientHttpService
    {
        private const string LOG_TAG = "[ClientHttpService]";

        // Default settings
        private static HttpRequestOptions _defaultOptions;
        private static Dictionary<string, string> _globalHeaders = new Dictionary<string, string>();

        #region Configuration

        /// <summary>
        /// Sets global default options for all requests
        /// </summary>
        public static void SetDefaultOptions(HttpRequestOptions options)
        {
            _defaultOptions = options?.Clone();
        }

        /// <summary>
        /// Sets a global header that will be added to all requests
        /// </summary>
        public static void SetGlobalHeader(string name, string value)
        {
            _globalHeaders[name] = value;
        }

        /// <summary>
        /// Removes a global header
        /// </summary>
        public static void RemoveGlobalHeader(string name)
        {
            _globalHeaders.Remove(name);
        }

        /// <summary>
        /// Clears all global headers
        /// </summary>
        public static void ClearGlobalHeaders()
        {
            _globalHeaders.Clear();
        }

        #endregion

        #region GET Methods

        /// <summary>  
        /// Sends a GET request and deserializes the response to the specified type.  
        /// </summary>  
        public static async UniTask<TResult> GetAsync<TResult>(
            string url,
            HttpRequestOptions requestOptions = null,
            IEnumerable<HttpRequestHeader> headers = null,
            Action<float> onDownloadProgress = null,
            CancellationToken cancellationToken = default) where TResult : class
        {
            return await SendAPIRequestAsync<TResult>(
                url, HTTPMethods.Get, requestOptions, null, headers, onDownloadProgress, cancellationToken);
        }

        /// <summary>  
        /// Sends a GET request and returns the response as ClientHttpResponseBase with strongly-typed data
        /// </summary>  
        public static async UniTask<TResponse> GetResponseAsync<TResponse>(
            string url,
            HttpRequestOptions requestOptions = null,
            IEnumerable<HttpRequestHeader> headers = null,
            Action<float> onDownloadProgress = null,
            CancellationToken cancellationToken = default)
            where TResponse : ClientHttpResponseBase, new()
        {
            return await SendRequestAsync<TResponse>(
                url, HTTPMethods.Get, requestOptions, null, headers, onDownloadProgress, cancellationToken);
        }

        /// <summary>  
        /// Sends a GET request and returns the response as plain text.  
        /// </summary>  
        public static async UniTask<ServiceResult<string>> GetTextAsync(
            string url,
            HttpRequestOptions requestOptions = null,
            Action<float> onDownloadProgress = null,
            IEnumerable<HttpRequestHeader> headers = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await SendRequestAsync<ClientHttpTextResponse>(
                    url, HTTPMethods.Get, GetEffectiveOptions(requestOptions), null, headers, onDownloadProgress, cancellationToken);

                if (response.IsSuccess)
                {
                    return ServiceResult<string>.Success(response.TextData);
                }

                return ServiceResult<string>.Failure(new HttpException(response.StatusCode, response.ErrorMessage));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return ServiceResult<string>.Failure(ex);
            }
        }

        #endregion

        #region POST Methods

        /// <summary>  
        /// Sends a POST request with a JSON body and deserializes the response.  
        /// </summary>  
        public static async UniTask<TResult> PostAsync<TResult>(
            string url,
            object jsonBody,
            HttpRequestOptions requestOptions = null,
            IEnumerable<HttpRequestHeader> headers = null,
            Action<float> onDownloadProgress = null,
            CancellationToken cancellationToken = default) where TResult : class
        {
            var (jsonBodyString, contentHeaders) = PrepareJsonBody(jsonBody, headers);
            return await SendAPIRequestAsync<TResult>(
                url, HTTPMethods.Post, requestOptions, jsonBodyString, contentHeaders, onDownloadProgress, cancellationToken);
        }

        /// <summary>  
        /// Sends a POST request with form data
        /// </summary>  
        public static async UniTask<TResult> PostFormAsync<TResult>(
            string url,
            Dictionary<string, string> formData,
            HttpRequestOptions requestOptions = null,
            IEnumerable<HttpRequestHeader> headers = null,
            Action<float> onDownloadProgress = null,
            CancellationToken cancellationToken = default) where TResult : class
        {
            var (formBodyString, contentHeaders) = PrepareFormBody(formData, headers);
            return await SendAPIRequestAsync<TResult>(
                url, HTTPMethods.Post, requestOptions, formBodyString, contentHeaders, onDownloadProgress, cancellationToken);
        }

        /// <summary>  
        /// Sends a POST request and returns strongly-typed response
        /// </summary>  
        public static async UniTask<TResponse> PostResponseAsync<TResponse>(
            string url,
            object jsonBody,
            HttpRequestOptions requestOptions = null,
            IEnumerable<HttpRequestHeader> headers = null,
            Action<float> onDownloadProgress = null,
            CancellationToken cancellationToken = default)
            where TResponse : ClientHttpResponseBase, new()
        {
            var (jsonBodyString, contentHeaders) = PrepareJsonBody(jsonBody, headers);
            return await SendRequestAsync<TResponse>(
                url, HTTPMethods.Post, requestOptions, jsonBodyString, contentHeaders, onDownloadProgress, cancellationToken);
        }

        #endregion

        #region PUT/PATCH Methods

        /// <summary>  
        /// Sends a PATCH request with a JSON body and deserializes the response.  
        /// </summary>  
        public static async UniTask<TResult> PatchAsync<TResult>(
            string url,
            object jsonBody,
            HttpRequestOptions requestOptions = null,
            IEnumerable<HttpRequestHeader> headers = null,
            Action<float> onDownloadProgress = null,
            CancellationToken cancellationToken = default) where TResult : class
        {
            var (jsonBodyString, contentHeaders) = PrepareJsonBody(jsonBody, headers);
            return await SendAPIRequestAsync<TResult>(
                url, HTTPMethods.Patch, requestOptions, jsonBodyString, contentHeaders, onDownloadProgress, cancellationToken);
        }

        /// <summary>  
        /// Sends a PUT request with a JSON body and deserializes the response.  
        /// </summary>  
        public static async UniTask<TResult> PutAsync<TResult>(
            string url,
            object jsonBody,
            HttpRequestOptions requestOptions = null,
            IEnumerable<HttpRequestHeader> headers = null,
            Action<float> onDownloadProgress = null,
            CancellationToken cancellationToken = default) where TResult : class
        {
            var (jsonBodyString, contentHeaders) = PrepareJsonBody(jsonBody, headers);
            return await SendAPIRequestAsync<TResult>(
                url, HTTPMethods.Put, requestOptions, jsonBodyString, contentHeaders, onDownloadProgress, cancellationToken);
        }

        #endregion

        #region DELETE Methods

        /// <summary>  
        /// Sends a DELETE request and deserializes the response.  
        /// </summary>  
        public static async UniTask<TResult> DeleteAsync<TResult>(
            string url,
            HttpRequestOptions requestOptions = null,
            IEnumerable<HttpRequestHeader> headers = null,
            Action<float> onDownloadProgress = null,
            CancellationToken cancellationToken = default) where TResult : class
        {
            return await SendAPIRequestAsync<TResult>(
                url, HTTPMethods.Delete, requestOptions, null, headers, onDownloadProgress, cancellationToken);
        }

        #endregion

        #region Media Download Methods

        /// <summary>  
        /// Downloads a texture from the specified URL.  
        /// </summary>  
        public static async UniTask<ServiceResult<Texture2D>> GetTextureAsync(
            string url,
            Texture2D textureSettings = null,
            HttpRequestOptions requestOptions = null,
            IEnumerable<HttpRequestHeader> headers = null,
            Action<float> onDownloadProgress = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await SendRequestAsync<ClientHttpTexture2DResponse>(
                    url, HTTPMethods.Get, GetEffectiveOptions(requestOptions), null, headers, onDownloadProgress, cancellationToken);

                if (response.IsSuccess && response.Texture2DBytes != null)
                {
                    var texture = textureSettings ?? new Texture2D(1, 1);
                    if (texture.LoadImage(response.Texture2DBytes))
                    {
                        return ServiceResult<Texture2D>.Success(texture);
                    }
                    else
                    {
                        Debug.LogError($"{LOG_TAG} Failed to load image data into texture from {url}");
                        return ServiceResult<Texture2D>.Failure(new InvalidOperationException("Failed to load image data"));
                    }
                }

                var error = new HttpException(response.StatusCode, response.ErrorMessage);
                return ServiceResult<Texture2D>.Failure(error);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return ServiceResult<Texture2D>.Failure(ex);
            }
        }

        /// <summary>
        /// Downloads binary data and returns the full response object
        /// </summary>
        public static async UniTask<ClientHttpBytesResponse> GetBytesResponseAsync(
            string url,
            HttpRequestOptions requestOptions = null,
            IEnumerable<HttpRequestHeader> headers = null,
            Action<float> onDownloadProgress = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await SendRequestAsync<ClientHttpBytesResponse>(
                    url, HTTPMethods.Get, GetEffectiveOptions(requestOptions), null, headers, onDownloadProgress, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return new ClientHttpBytesResponse
                {
                    IsSuccess = false,
                    StatusCode = 0,
                    ErrorMessage = ex.Message
                };
            }
        }

        #endregion

        #region Streaming Methods

        /// <summary>
        /// Downloads data with streaming support
        /// </summary>
        public static async UniTask<ServiceResult<byte[]>> GetStreamingAsync(
            string url,
            Action<byte[]> onChunkReceived,
            Action<long, long> onProgressChanged = null,
            Action onCompleted = null,
            HttpRequestOptions requestOptions = null,
            IEnumerable<HttpRequestHeader> headers = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = CreateRequest(url, HTTPMethods.Get, GetEffectiveOptions(requestOptions), null, headers);
                request.EnableStreaming(onChunkReceived, onProgressChanged, onCompleted);
                var httpResponse = await request.SendAsync(cancellationToken);

                if (httpResponse.IsSuccess)
                {
                    var accumulatedData = request.GetAccumulatedData();
                    return ServiceResult<byte[]>.Success(accumulatedData);
                }

                return ServiceResult<byte[]>.Failure(new HttpException(httpResponse.StatusCode, httpResponse.Error));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return ServiceResult<byte[]>.Failure(ex);
            }
        }

        #endregion

        #region Private Helper Methods

        private static HTTPRequest CreateRequest(
            string url,
            HTTPMethods method,
            HttpRequestOptions options,
            string body,
            IEnumerable<HttpRequestHeader> headers)
        {
            var request = new HTTPRequest(url, method, options);

            // Add global headers first
            foreach (var globalHeader in _globalHeaders)
            {
                request.SetHeader(globalHeader.Key, globalHeader.Value);
            }

            // Add request-specific headers (will override global headers if same key)
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.SetHeader(header.Name, header.Value);
                }
            }

            // Add body if provided
            if (!string.IsNullOrEmpty(body))
            {
                request.SetBody(body);
            }

            return request;
        }

        private static async UniTask<T> SendRequestAsync<T>(string url,
                                                            HTTPMethods method,
                                                            HttpRequestOptions options,
                                                            string body,
                                                            IEnumerable<HttpRequestHeader> headers,
                                                            Action<float> onDownloadProgress,
                                                            CancellationToken cancellationToken = default) where T : ClientHttpResponseBase, new()
        {
            var request = CreateRequest(url, method, options, body, headers);
            if (request == null)
            {
                throw new InvalidOperationException("Failed to create an HTTP request.");
            }

            if (onDownloadProgress != null)
            {
                request.OnDownloadProgress(onDownloadProgress);
            }

            try
            {
                // Send the request
                HTTPResponse httpResponse = await request.SendAsync(cancellationToken);
                T respone = new();
                respone.FromHttpResponse(httpResponse);
                return respone;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SendRequestAsync] Error sending request to {url}: {ex.Message}");
                throw;
            }
        }

        /// <summary>  
        /// Sends an API request and deserializes the JSON response.  
        /// </summary>  
        private static async UniTask<T> SendAPIRequestAsync<T>(
            string url,
            HTTPMethods httpMethods,
            HttpRequestOptions requestOptions,
            string body,
            IEnumerable<HttpRequestHeader> headers,
            Action<float> onDownloadProgress,
            CancellationToken cancellationToken = default) where T : class
        {
            var response = await SendRequestAsync<ClientHttpTextResponse>(
                url, httpMethods, GetEffectiveOptions(requestOptions), body, headers, onDownloadProgress, cancellationToken);

            if (!response.IsSuccess)
            {
                Debug.LogError($"{LOG_TAG} API request failed: {response.ErrorMessage} (Status: {response.StatusCode})");
                throw new HttpException(response.StatusCode, response.ErrorMessage);
            }

            try
            {
                if (string.IsNullOrEmpty(response.TextData))
                {
                    Debug.LogWarning($"{LOG_TAG} Empty response from {url}");
                    return default;
                }

                var result = JsonConvert.DeserializeObject<T>(response.TextData);
                return result;
            }
            catch (JsonException ex)
            {
                var preview = response.TextData?.Substring(0, Math.Min(response.TextData?.Length ?? 0, 200));
                Debug.LogError($"{LOG_TAG} JSON deserialization error for {url}: {ex.Message}\nResponse preview: {preview}...");
                throw new HttpException(response.StatusCode, "JSON deserialization failed", ex);
            }
        }

        private static (string bodyString, IEnumerable<HttpRequestHeader> headers) PrepareJsonBody(
            object jsonBody,
            IEnumerable<HttpRequestHeader> headers)
        {
            string jsonBodyString = null;
            if (jsonBody != null)
            {
                try
                {
                    jsonBodyString = JsonConvert.SerializeObject(jsonBody);
                    // Validate JSON by attempting to deserialize
                    JsonConvert.DeserializeObject(jsonBodyString);
                }
                catch (JsonException ex)
                {
                    Debug.LogError($"{LOG_TAG} Invalid JSON body: {ex.Message}");
                    throw new ArgumentException("Invalid JSON body", nameof(jsonBody), ex);
                }
            }

            // Add JSON content type header if not present
            var headersList = headers?.ToList() ?? new List<HttpRequestHeader>();
            if (!headersList.Any(h => h.Name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)))
            {
                headersList.Add(HttpRequestHeader.JsonContent());
            }

            return (jsonBodyString, headersList);
        }

        private static (string formString, IEnumerable<HttpRequestHeader> headers) PrepareFormBody(Dictionary<string, string> formData, IEnumerable<HttpRequestHeader> headers)
        {
            string formBodyString = null;

            // Build the form body string
            if (formData != null && formData.Count > 0)
            {
                var formPairs = new List<string>();
                foreach (var kv in formData)
                {
                    string key = Uri.EscapeDataString(kv.Key);
                    string value = Uri.EscapeDataString(kv.Value);
                    formPairs.Add($"{key}={value}");
                }
                formBodyString = string.Join("&", formPairs);
            }

            // Add form content type header if not present
            var headersList = headers != null ? new List<HttpRequestHeader>(headers) : new List<HttpRequestHeader>();
            bool hasContentTypeHeader = false;

            foreach (var header in headersList)
            {
                if (header.Name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    hasContentTypeHeader = true;
                    break;
                }
            }

            if (!hasContentTypeHeader)
            {
                headersList.Add(new HttpRequestHeader("Content-Type", "application/x-www-form-urlencoded"));
            }

            return (formBodyString, headersList);
        }

        private static HttpRequestOptions GetEffectiveOptions(HttpRequestOptions requestOptions)
        {
            if (requestOptions != null)
                return requestOptions;

            return _defaultOptions ?? HttpRequestOptions.Default;
        }

        #endregion
    }

    /// <summary>
    /// Service result wrapper for better error handling
    /// </summary>
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T Data { get; private set; }
        public Exception Error { get; private set; }
        public string ErrorMessage => Error?.Message;

        private ServiceResult(bool isSuccess, T data, Exception error)
        {
            IsSuccess = isSuccess;
            Data = data;
            Error = error;
        }

        public static ServiceResult<T> Success(T data)
        {
            return new ServiceResult<T>(true, data, null);
        }

        public static ServiceResult<T> Failure(Exception error)
        {
            return new ServiceResult<T>(false, default(T), error);
        }

        public static ServiceResult<T> Failure(string errorMessage)
        {
            return new ServiceResult<T>(false, default(T), new Exception(errorMessage));
        }
    }
}