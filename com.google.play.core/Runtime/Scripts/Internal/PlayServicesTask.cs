// Copyright 2019 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using UnityEngine;

namespace Google.Play.Core.Internal
{ 
    /// <summary>
    /// Wraps Play Services Task which represent asynchronous operations.
    /// Allows C# classes to register callbacks for when a task succeeds or fails.
    /// </summary>
    /// <typeparam name="TAndroidJava">
    /// The type of the object that the underlying Play Services Task will provide, once it completes.
    /// Must be a primitive type (e.g. bool, int, or float) or an AndroidJavaObject.
    /// </typeparam>
    public class PlayServicesTask<TAndroidJava> : IDisposable
    {
        private readonly AndroidJavaObject _javaTask;
        
        #if UNITY_6000_OR_NEWER
        private bool _isDisposed;
        #endif

        public PlayServicesTask(AndroidJavaObject javaTask)
        {
            if (javaTask == null)
            {
                throw new ArgumentNullException("javaTask must not be null.");
            }

            _javaTask = javaTask;
            
            #if UNITY_6000_OR_NEWER
            Debug.Log($"Initializing PlayServicesTask for type {typeof(TAndroidJava)}");
            #endif
        }

        /// <summary>
        /// Register a callback that will fire when the underlying Play Services Task succeeds.
        /// </summary>
        /// <param name="onSuccess">
        /// The action that will be invoked with the result of the underlying Play Services Task.
        /// </param>
        public void RegisterOnSuccessCallback(Action<TAndroidJava> onSuccess)
        {
            #if UNITY_6000_OR_NEWER
            if (_isDisposed)
            {
                Debug.LogError("Cannot register callback on disposed PlayServicesTask");
                return;
            }
            #endif

            var listenerProxy = new TaskOnSuccessListener<TAndroidJava>();
            listenerProxy.OnTaskSucceeded += onSuccess;
            AddOnSuccessListener(listenerProxy);
        } 

        /// <summary>
        /// Register a callback that will fire when the underlying Play Services Task fails.
        /// </summary>
        /// <param name="onFailure">
        /// The action that will be invoked when the underlying Play Services Task fails.
        /// The action will be passed a human-readable string describing the exception which caused the failure,
        /// along with a corresponding error code.
        /// </param>
        public void RegisterOnFailureCallback(Action<string, int> onFailure)
        {
            var listenerProxy = new TaskOnFailureListener();
            listenerProxy.OnTaskFailed += onFailure;
            AddOnFailureListener(listenerProxy);
        }

        private void AddOnSuccessListener(AndroidJavaProxy listenerProxy)
        {
            #if UNITY_6000_OR_NEWER
            try
            {
                using (var result = _javaTask.Call<AndroidJavaObject>("addOnSuccessListener", listenerProxy))
                {
                    // Result automatically disposed by using block
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Unity 6000+: Error adding success listener: {e.Message}");
            }
            #else
            _javaTask.Call<AndroidJavaObject>("addOnSuccessListener", listenerProxy).Dispose();
            #endif
        }

        private void AddOnFailureListener(AndroidJavaProxy listenerProxy)
        {
            #if UNITY_6000_OR_NEWER
            try
            {
                using (var result = _javaTask.Call<AndroidJavaObject>("addOnFailureListener", listenerProxy))
                {
                    // Result automatically disposed by using block
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Unity 6000+: Error adding failure listener: {e.Message}");
            }
            #else
            _javaTask.Call<AndroidJavaObject>("addOnFailureListener", listenerProxy).Dispose();
            #endif
        }

        public void Dispose()
        {
            #if UNITY_6000_OR_NEWER
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            #endif

            _javaTask.Dispose();
        }
    }
}