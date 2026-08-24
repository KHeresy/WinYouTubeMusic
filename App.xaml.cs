using System.Windows;
using WinYouTubeMusic.Native;

namespace WinYouTubeMusic
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppIdentityHelper.Initialize();
            base.OnStartup(e);
        }
    }
}
