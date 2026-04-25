// src/WormWinForms/Input.cs
using System.Collections.Generic;
using System.Windows.Forms;

namespace WormWinForms.Input
{
    public static class Input
    {
        private static readonly Dictionary<Keys, bool> _keys = new();

        public static void ChangeState(Keys key, bool down) => _keys[key] = down;

        public static bool IsPressed(Keys key) =>
            _keys.TryGetValue(key, out bool state) && state;
    }
}
