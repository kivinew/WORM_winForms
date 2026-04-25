// src/WormWinForms/Network/TcpGameClient.cs
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WormWinForms.Model;

namespace WormWinForms.Network
{
    public sealed class TcpGameClient : ITcpGameClient, IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private TcpClient _client;
        private NetworkStream _stream;

        public TcpGameClient(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public async Task ConnectAsync(GameState state)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(_host, _port);
            _stream = _client.GetStream();

            // Пример: каждые 100 мс отправляем позицию головы + направление
            var timer = new System.Threading.Timer(
                async _ => await SendStateAsync(state),
                null, 0, 100);
        }

        private async Task SendStateAsync(GameState state)
        {
            var head = state.Worm[0];
            var payload = $"{head.X},{head.Y},{(int)state.CurrentDirection}";
            var data = Encoding.UTF8.GetBytes(payload);
            await _stream.WriteAsync(data, 0, data.Length);
        }

        public async Task DisconnectAsync()
        {
            if (_stream != null) await _stream.DisposeAsync();
            _client?.Close();
        }

        public void Dispose() => DisconnectAsync().GetAwaiter().GetResult();
    }
}
