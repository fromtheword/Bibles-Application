using System;
using System.Threading.Tasks;

namespace GeneralExtensions
{
    public static class TaskExtensions
    {
        public static async void SafeFireAndForget(this Task task, bool returnToCallingContent, Action<Exception> onException = null)
        {
            try
            {
                await task.ConfigureAwait(returnToCallingContent);
            }
            catch (Exception err) when (onException != null)
            {
                onException(err);
            }
        }
    }
}
