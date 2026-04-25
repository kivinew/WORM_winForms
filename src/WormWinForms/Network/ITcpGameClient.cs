// src/WormWinForms/Network/ITcpGameClient.cs
using System.Threading.Tasks;
using WormWinForms.Model;

namespace WormWinForms.Network
{
    public interface ITcpGameClient
    {
        Task ConnectAsync(GameState state);
        Task DisconnectAsync();
    }
}
