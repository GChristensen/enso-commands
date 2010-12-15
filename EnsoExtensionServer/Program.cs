using System;
using System.Configuration;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace EnsoExtension
{
    static class Program
    {
        [DllImport("user32", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private static void WaitForEnso()
        {
            const int MAX_TRIES = 180;

            int i = 0;
            IntPtr hWnd;

            for (; i < MAX_TRIES; ++i)
            {
                hWnd = FindWindow("HumanizedMessageWindow", null);
                if (hWnd != IntPtr.Zero)
                    break;
                Thread.Sleep(1000);
            }

            if (i == MAX_TRIES)
                Environment.Exit(0);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            WaitForEnso();

            EnsoExtensionServer server = new EnsoExtensionServer();

            EnsoExtensionsSection section = (EnsoExtensionsSection)ConfigurationManager.GetSection("ensoExtensions");
            foreach (EnsoExtensionElement element in section.EnsoExtensions)
            {
                try
                {
                    Type type = Type.GetType(element.Type);
                    IEnsoExtension extension = (IEnsoExtension)Activator.CreateInstance(type);
                    server.RegisterExtension(extension);
                }
                catch (Exception e)
                {
                    Debug.Fail(e.Message);
                }
            }

            Application.Run();
        }
    }
}
