using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Text;

namespace Core
{
    public class ClientHttpJSONResponse<T> : ClientHttpResponseBase where T : class, new()
    {
        public T ParsedData { get; private set; }

        public override void OnHandleResponse(HTTPResponse response)
        {
            if (response != null && response.IsSuccess && response.Data != null && response.Data.Length > 0)
            {
                string jsonText = Encoding.UTF8.GetString(response.Data);
                ParsedData = JsonConvert.DeserializeObject<T>(jsonText);
            }
            else if (response != null)
            {
                StatusCode = response.StatusCode;

                if (!response.IsSuccess)
                {
                    ErrorMessage = $"HTTP error: {response.StatusCode}";
                    if (!string.IsNullOrEmpty(response.Error))
                    {
                        ErrorMessage += $" - {response.Error}";
                    }
                }
                else if (response.Data == null || response.Data.Length == 0)
                {
                    ErrorMessage = "Response contained no JSON data";
                }

                Debug.LogWarning($"JSON response error: {ErrorMessage}");
                IsSuccess = false;
                Data = null;
            }
            else
            {
                ErrorMessage = "Response was null";
                IsSuccess = false;
                Data = null;
                Debug.LogError("Response object was null");
            }
        }
    }
}