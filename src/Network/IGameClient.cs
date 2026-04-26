using System.Threading.Tasks;
using SnakeNet.Model;

namespace SnakeNet.Network
{
    public interface IGameClient
    {
        Task ConnectAsync(GameState state);
        Task DisconnectAsync();
    }
}
