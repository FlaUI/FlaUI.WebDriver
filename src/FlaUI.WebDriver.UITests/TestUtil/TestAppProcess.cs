using System;
using System.Diagnostics;

namespace FlaUI.WebDriver.UITests.TestUtil
{
    public class TestAppProcess : IDisposable
    {
        private readonly Process _process;

        public TestAppProcess()
        {
            _process = Process.Start(TestApplication.FullPath);
            while (_process.MainWindowHandle == IntPtr.Zero)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        public Process Process => _process;

        public void Dispose()
        {
            _process.Kill();
            _process.Dispose();
        }
    }
}
