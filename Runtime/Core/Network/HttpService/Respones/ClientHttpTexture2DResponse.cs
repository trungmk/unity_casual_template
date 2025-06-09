using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public class ClientHttpTexture2DResponse : ClientHttpResponseBase
    {
        public byte[] Texture2DBytes { get; private set; }

        public override void OnHandleResponse(HTTPResponse response)
        {
            try
            {
                if (response != null && response.IsSuccess && response.Data != null && response.Data.Length > 0)
                {
                    Texture2DBytes = response.Data;
                }
                else if (response != null)
                {
                    ErrorMessage = response.Error ?? "No image data received";
                    StatusCode = response.StatusCode;
                    IsSuccess = false;
                }
                else
                {
                    ErrorMessage = "Response was null";
                    IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                ErrorMessage = $"Error processing image: {ex.Message}";
                IsSuccess = false;
            }
        }
    }
}
