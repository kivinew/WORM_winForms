using System.Collections;
using System.Windows.Forms;

namespace Worm_WinForms
{
    public static class Input
    {
        private static readonly Hashtable Keys = new Hashtable();

        public static void ChangeState(Keys key, bool state)
        {
            Keys[key] = state;
        }

        public static bool Press(Keys key)
        {
            if (Keys[key] == null)
                Keys[key] = false;
            return (bool)Keys[key];
        }
    }
}
