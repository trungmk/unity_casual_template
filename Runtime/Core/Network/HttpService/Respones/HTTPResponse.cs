using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Core
{
    public class HTTPResponse
    {
        public long StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public byte[] Data { get; set; }
        public string Text { get; set; }
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public bool IsCanceled { get; set; }
        public bool IsCached { get; set; }
        public long ResponseTimeMs { get; set; }
        public long ContentLength => Data?.Length ?? 0;
        public string ContentType => GetHeader("Content-Type");
        public Encoding TextEncoding { get; set; } = Encoding.UTF8;

        #region Status Code Helpers

        public bool IsSuccessStatusCode => StatusCode >= 200 && StatusCode < 300;
        public bool IsRedirection => StatusCode >= 300 && StatusCode < 400;
        public bool IsClientError => StatusCode >= 400 && StatusCode < 500;
        public bool IsServerError => StatusCode >= 500 && StatusCode < 600;
        public bool IsTimeout => StatusCode == 408 || Error?.ToLower().Contains("timeout") == true;
        public bool IsNetworkError => !IsSuccess && (StatusCode == 0 || Error?.ToLower().Contains("network") == true);

        #endregion

        #region Header Helpers

        public string GetHeader(string name)
        {
            if (Headers == null) return null;

            foreach (var key in Headers.Keys)
            {
                if (string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
                {
                    return Headers[key];
                }
            }

            return null;
        }

        public IEnumerable<string> GetHeaders(string name)
        {
            if (Headers == null) yield break;

            foreach (var header in Headers)
            {
                if (string.Equals(header.Key, name, StringComparison.OrdinalIgnoreCase))
                {
                    yield return header.Value;
                }
            }
        }

        public bool HasHeader(string name)
        {
            return GetHeader(name) != null;
        }

        #endregion

        #region Content Helpers

        public string GetText()
        {
            if (!string.IsNullOrEmpty(Text))
                return Text;

            if (Data != null && Data.Length > 0)
            {
                return TextEncoding.GetString(Data);
            }

            return null;
        }

        public T GetJson<T>()
        {
            try
            {
                string content = GetText();
                if (string.IsNullOrEmpty(content))
                    return default(T);

                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (JsonException ex)
            {
                Debug.LogError($"[HTTPResponse] Failed to deserialize JSON: {ex.Message}");
                throw new InvalidOperationException($"Failed to deserialize JSON response: {ex.Message}", ex);
            }
        }

        public bool TryGetJson<T>(out T result)
        {
            try
            {
                result = GetJson<T>();
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }

        public Texture2D GetTexture()
        {
            if (Data == null || Data.Length == 0)
                return null;

            try
            {
                var texture = new Texture2D(1, 1);
                if (texture.LoadImage(Data))
                    return texture;

                UnityEngine.Object.DestroyImmediate(texture);
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HTTPResponse] Failed to create texture: {ex.Message}");
                return null;
            }
        }

        public void SaveToFile(string filePath)
        {
            if (Data == null)
                throw new InvalidOperationException("No data to save");

            try
            {
                System.IO.File.WriteAllBytes(filePath, Data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HTTPResponse] Failed to save file: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Factory Methods

        public static HTTPResponse CreateSuccess(string content = null, long statusCode = 200)
        {
            return new HTTPResponse
            {
                StatusCode = statusCode,
                StatusMessage = GetStatusMessage(statusCode),
                IsSuccess = true,
                Text = content,
                Data = content != null ? Encoding.UTF8.GetBytes(content) : null
            };
        }

        public static HTTPResponse CreateError(string errorMessage, long statusCode = 0, bool isCanceled = false)
        {
            return new HTTPResponse
            {
                StatusCode = statusCode,
                StatusMessage = GetStatusMessage(statusCode),
                Error = errorMessage,
                IsSuccess = false,
                IsCanceled = isCanceled
            };
        }

        public static HTTPResponse CreateTimeout(string message = "Request timeout")
        {
            return CreateError(message, 408, false);
        }

        public static HTTPResponse CreateCanceled(string message = "Request was canceled")
        {
            return CreateError(message, 0, true);
        }

        #endregion

        #region Utility Methods

        public static string GetStatusMessage(long statusCode)
        {
            return statusCode switch
            {
                200 => "OK",
                201 => "Created",
                204 => "No Content",
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                408 => "Request Timeout",
                429 => "Too Many Requests",
                500 => "Internal Server Error",
                502 => "Bad Gateway",
                503 => "Service Unavailable",
                504 => "Gateway Timeout",
                _ => "Unknown"
            };
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"HTTPResponse:");
            sb.AppendLine($"  Status: {StatusCode} {StatusMessage}");
            sb.AppendLine($"  Success: {IsSuccess}");
            sb.AppendLine($"  ContentLength: {ContentLength} bytes");
            sb.AppendLine($"  ContentType: {ContentType ?? "N/A"}");
            sb.AppendLine($"  ResponseTime: {ResponseTimeMs}ms");

            if (!string.IsNullOrEmpty(Error))
                sb.AppendLine($"  Error: {Error}");

            if (IsCanceled)
                sb.AppendLine($"  Canceled: true");

            if (IsCached)
                sb.AppendLine($"  Cached: true");

            return sb.ToString();
        }

        #endregion
    }
}