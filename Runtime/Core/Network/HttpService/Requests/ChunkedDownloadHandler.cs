using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Core
{
    public class ChunkedDownloadHandler : DownloadHandlerScript
    {
        // Events
        private readonly Action<byte[]> _onChunkReceived;
        private readonly Action<long, long> _onProgressChanged; // received, total
        private readonly Action _onCompleted;
        private readonly Action<string> _onError;

        // Data tracking
        private List<byte> _accumulatedData;
        private long _totalBytesReceived;
        private long _totalContentLength;
        private bool _isCompleted;
        private bool _hasError;

        // Configuration
        private readonly bool _accumulate;

        public ChunkedDownloadHandler(
            Action<byte[]> onChunkReceived,
            Action<long, long> onProgressChanged = null,
            Action onCompleted = null,
            Action<string> onError = null,
            bool accumulate = false,
            int bufferSize = 4096) : base(new byte[bufferSize])
        {
            _onChunkReceived = onChunkReceived;
            _onProgressChanged = onProgressChanged;
            _onCompleted = onCompleted;
            _onError = onError;
            _accumulate = accumulate;

            if (_accumulate)
            {
                _accumulatedData = new List<byte>();
            }

            _totalBytesReceived = 0;
            _totalContentLength = -1;
            _isCompleted = false;
            _hasError = false;
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (_hasError || _isCompleted)
            {
                return false;
            }

            if (data == null || dataLength <= 0)
            {
                Debug.LogWarning("[ChunkedDownloadHandler] Received empty or invalid data chunk.");
                return true; // Continue anyway
            }

            try
            {
                // Create chunk with exact size
                byte[] chunk = new byte[dataLength];
                Array.Copy(data, chunk, dataLength);
                _totalBytesReceived += dataLength;

                if (_accumulate)
                {
                    _accumulatedData.AddRange(chunk);
                }
                _onChunkReceived?.Invoke(chunk);
                _onProgressChanged?.Invoke(_totalBytesReceived, _totalContentLength);

                return true;
            }
            catch (Exception ex)
            {
                _hasError = true;
                Debug.LogError($"[ChunkedDownloadHandler] Error processing chunk: {ex.Message}");
                _onError?.Invoke(ex.Message);
                return false;
            }
        }

        protected override void ReceiveContentLengthHeader(ulong contentLength)
        {
            _totalContentLength = (long)contentLength;
            _onProgressChanged?.Invoke(0, _totalContentLength);
        }

        protected override void CompleteContent()
        {
            if (_hasError)
            {
                return;
            }

            _isCompleted = true;
            _onProgressChanged?.Invoke(_totalBytesReceived, _totalContentLength);
            _onCompleted?.Invoke();
        }

        protected override byte[] GetData()
        {
            if (_accumulate && _accumulatedData != null)
            {
                return _accumulatedData.ToArray();
            }

            // Return null for streaming-only mode to save memory
            return null;
        }

        protected override string GetText()
        {
            if (_accumulate && _accumulatedData != null)
            {
                return System.Text.Encoding.UTF8.GetString(_accumulatedData.ToArray());
            }

            return null;
        }

        protected override float GetProgress()
        {
            if (_totalContentLength <= 0)
            {
                return 0f;
            }

            return (float)_totalBytesReceived / _totalContentLength;
        }

        // Public properties for monitoring
        public long TotalBytesReceived => _totalBytesReceived;
        public long TotalContentLength => _totalContentLength;
        public bool IsCompleted => _isCompleted;
        public bool HasError => _hasError;
        public float Progress => GetProgress();

        // Manual error handling
        public void SetError(string errorMessage)
        {
            if (!_hasError)
            {
                _hasError = true;
                Debug.LogError($"[ChunkedDownloadHandler] Manual error: {errorMessage}");
                _onError?.Invoke(errorMessage);
            }
        }

        public override void Dispose()
        {
            try
            {
                _accumulatedData?.Clear();
                _accumulatedData = null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChunkedDownloadHandler] Error during disposal: {ex.Message}");
            }
            finally
            {
                base.Dispose();
            }
        }
    }
}
