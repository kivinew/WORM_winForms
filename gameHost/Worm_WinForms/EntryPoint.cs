using System;
using System.Drawing;
using System.Windows.Forms;

namespace Worm_WinForms
{
    internal static class EntryPoint
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WormGame(new Size(15,15)));
        }
    }
}
