using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MapMaker.Editor.Services
{
    public static class LoadingService
    {
        public static event Action<bool, string> OnLoadingChanged;

        public static async Task Show(string message = "Loading...")
        {
            OnLoadingChanged?.Invoke(true, message);
            await Dispatcher.Yield(DispatcherPriority.Background);
        }
        public static void Hide()
        {
            OnLoadingChanged?.Invoke(false, string.Empty);
        }
    }
}
