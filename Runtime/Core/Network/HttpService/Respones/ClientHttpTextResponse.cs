using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public class ClientHttpTextResponse : ClientHttpResponseBase
    {
        public string TextData { get; private set; }

        public override void OnHandleResponse(HTTPResponse response)
        {
            if (response != null && response.IsSuccess && response.Data != null && response.Data.Length > 0)
            {
                TextData = Encoding.UTF8.GetString(response.Data);
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
                    ErrorMessage = "Response contained no data";
                }

                Debug.LogWarning($"Text response error: {ErrorMessage}");
                IsSuccess = false;
                TextData = null;
            }
            else
            {
                ErrorMessage = "Response was null";
                IsSuccess = false;
                TextData = null;
                Debug.LogError("Response object was null");
            }
        }
    }
}
