using System;
using UnityEngine;

namespace Core
{
    /// <summary>  
    /// Response that contains binary data  
    /// </summary>  
    public class ClientHttpBytesResponse : ClientHttpResponseBase
    {
        /// <summary>  
        /// Process the response and store the binary data  
        /// </summary>  
        public override void OnHandleResponse(HTTPResponse response)
        {
            if (response.IsSuccess && response.Data != null)
            {
                Data = response.Data;
                if (response.Data.Length > 0)
                {
                    // Check if this might be an HTML error page instead of binary data  
                    if (response.Data.Length < 1000)
                    {
                        string preview = System.Text.Encoding.UTF8.GetString(response.Data, 0, System.Math.Min(100, response.Data.Length));
                        if (preview.Contains("<html") || preview.Contains("<!DOCTYPE"))
                        {
                            Debug.LogWarning($"[ClientHttpBytesResponse] Received HTML content instead of binary data: {preview}");
                        }
                    }
                }
            }
            else if (!response.IsSuccess)
            {
                Debug.LogError($"[ClientHttpBytesResponse] Failed to download binary data: {response.Error}, StatusCode={response.StatusCode}");
            }
        }
    }
}