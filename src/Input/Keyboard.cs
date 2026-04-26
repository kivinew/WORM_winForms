using System.Collections.Generic;
using System.Windows.Forms;

namespace SnakeNet.Input
{
    public static class Keyboard
    {
        private static readonly Dictionary<Keys, bool> _state = new();

        public static void ChangeState(Keys key, bool down) => _state[key] = down;

        public static bool IsDown(Keys key) =>
            _state.TryGetValue(key, out bool val) && val;
    }
}
