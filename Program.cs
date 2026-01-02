using System;
using System.Windows.Forms;
using GreenLifeOrganicStore.Forms;

namespace GreenLifeOrganicStore
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}

// commit 20: feat: introduce Program support

// commit 28: style: clean up Program

// commit 36: chore: update Program

// commit 44: feat: add Program functionality
