using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Drboum.Utilities
{
    public static class TaskHelper
    {
        public static Task LogException(this Task task)
        {
            task.ContinueWith(LogTaskException);
            return task;
        }

        public static Task<T> LogException<T>(this Task<T> task)
        {
            task.ContinueWith(LogTaskException);
            return task;
        }

        private static void LogTaskException(Task t)
        {
            if ( t.IsFaulted && t.Exception != null )
            {
                Exception exception = t.Exception;
                while ( exception.InnerException != null )
                {
                    exception = exception.InnerException;
                }
                Debug.LogException(exception);
            }
        }
    }
}