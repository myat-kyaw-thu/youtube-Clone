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

// micro-commit 27: chore: update Program dependencies

// micro-commit 31: docs: add usage example for Program

// micro-commit 35: style: align braces in Program
