using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Core;
using UnityEngine;

namespace Core
{
    // Extension method cho UnityWebRequestAsyncOperation  
    public static class UnityWebRequestAsyncOperationExtensions
    {
        public static async UniTask<UnityWebRequestAsyncOperation> ToUniTask(
            this UnityWebRequestAsyncOperation asyncOp,
            Action<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            float lastProgress = 0;
            while (!asyncOp.isDone)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (progress != null && asyncOp.progress != lastProgress)
                {
                    progress(asyncOp.progress);
                    lastProgress = asyncOp.progress;
                }

                await UniTask.Yield();
            }

            return asyncOp;
        }
    }
}