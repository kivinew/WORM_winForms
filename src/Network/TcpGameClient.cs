using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SnakeNet.Model;

namespace SnakeNet.Network
{
    public sealed class TcpGameClient : IGameClient, IDisposable
    {
        private readonly string _host;
        private readonly int    _port;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private System.Threading.Timer? _timer;               // отправка каждые 100 мс
        private CancellationTokenSource? _cts;

        public event Action? ServerDisconnected;
        public event Action<int, int, Direction, int>? ServerPositionReceived;

        public TcpGameClient(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public async Task ConnectAsync(GameState state)
        {
            if (_client != null && _client.Connected)
                throw new InvalidOperationException("Клиент уже подключен");
            
            _client = new TcpClient();
            await _client.ConnectAsync(_host, _port);
            _stream = _client.GetStream();

            _cts = new CancellationTokenSource();

            // каждые 100 мс отправляем позицию и направление
            _timer = new System.Threading.Timer(async _ => 
            {
                try
                {
                    await SendAsync(state);
                }
                catch
                {
                    // При ошибке отправки останавливаем таймер и отключаемся
                    _timer?.Dispose();
                    await DisconnectAsync();
                }
            }, null, 0, 100);

            // Запускаем чтение данных от сервера
            _ = Task.Run(async () =>
            {
                var buffer = new byte[1024];
                try
                {
                    while (!_cts.IsCancellationRequested && _client.Connected)
                    {
                        var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, _cts.Token);
                        if (bytesRead == 0) break; // Сервер отключился

                        var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        var parts = data.Split(',');
                        if (parts.Length == 6 && 
                            int.TryParse(parts[0], out int x) && 
                            int.TryParse(parts[1], out int y) && 
                            int.TryParse(parts[2], out int dir) &&
                            int.TryParse(parts[3], out int length) &&
                            int.TryParse(parts[4], out int foodX) &&
                            int.TryParse(parts[5], out int foodY))
                        {
                            ServerPositionReceived?.Invoke(x, y, (Direction)dir, length);
                            // Синхронизируем положение еды с сервера
                            state.Food = new Square(foodX, foodY);
                        }
                    }
                }
                catch
                {
                    // Нормальное закрытие или ошибка соединения
                }
                finally
                {
                    await DisconnectAsync();
                    ServerDisconnected?.Invoke();
                }
            }, _cts.Token);
        }

        private bool _foodEatenFlag = false;

        private async Task SendAsync(GameState state)
        {
            if (_stream == null || _client == null || !_client.Connected)
                return;
            
            var head = state.Worm[0];
            var payload = $"{head.X},{head.Y},{(int)state.CurrentDirection},{state.Worm.Count},{(_foodEatenFlag ? 1 : 0)}";
            var data = Encoding.UTF8.GetBytes(payload);
            await _stream.WriteAsync(data, 0, data.Length);

            _foodEatenFlag = false;
        }

        public void NotifyFoodEaten()
        {
            _foodEatenFlag = true;
        }

        public async Task DisconnectAsync()
        {
            _cts?.Cancel();
            _timer?.Dispose();
            
            if (_stream != null) 
            {
                await _stream.DisposeAsync();
                _stream = null;
            }
            
            if (_client != null)
            {
                _client.Close();
                _client.Dispose();
                _client = null;
            }
            
            // Обнуляем ссылки для возможности повторного подключения
            _cts = null;
            _timer = null;
            
            // Очищаем все подписчики событий чтобы не было дубликатов при повторном подключении
            ServerDisconnected = null;
            ServerPositionReceived = null;
        }

        public void Dispose() => DisconnectAsync().GetAwaiter().GetResult();
    }
}
