using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SnakeNet.Model;

namespace SnakeNet.Network
{
    public sealed class TcpGameServer : IGameClient, IDisposable
    {
        private readonly int _port;
        private TcpListener? _listener;
        private TcpClient? _connectedClient;
        private NetworkStream? _stream;
        private System.Threading.Timer? _timer;
        private CancellationTokenSource? _cts;

        public bool IsRunning { get; private set; }
        public event Action? ClientConnected;
        public event Action? ClientDisconnected;
        public event Action<int, int, Direction, int>? ClientPositionReceived;
        public event Action? ClientAteFood;

        public TcpGameServer(int port)
        {
            _port = port;
        }

        public async Task ConnectAsync(GameState state)
        {
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
            _listener.Start();
            IsRunning = true;

            // Принимаем подключение в фоне
            _ = Task.Run(async () => 
            {
                try
                {
                    while (!_cts.IsCancellationRequested)
                    {
                        _connectedClient = await _listener.AcceptTcpClientAsync(_cts.Token);
                        _stream = _connectedClient.GetStream();
                        
                        ClientConnected?.Invoke();

                        // каждые 100 мс отправляем состояние игры
                        _timer = new System.Threading.Timer(async _ => await SendAsync(state),
                                            null, 0, 100);

                        // Запускаем чтение данных от клиента
                        _ = Task.Run(async () =>
                        {
                            var buffer = new byte[1024];
                            try
                            {
                                while (!_cts.IsCancellationRequested && _connectedClient.Connected)
                                {
                                    var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, _cts.Token);
                                    if (bytesRead == 0) break; // Клиент отключился

                                    var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                    var parts = data.Split(',');
                                    if (parts.Length == 5 && 
                                        int.TryParse(parts[0], out int x) && 
                                        int.TryParse(parts[1], out int y) && 
                                        int.TryParse(parts[2], out int dir) &&
                                        int.TryParse(parts[3], out int length) &&
                                        int.TryParse(parts[4], out int foodEaten))
                                    {
                                        ClientPositionReceived?.Invoke(x, y, (Direction)dir, length);
                                        
                                        if (foodEaten == 1)
                                        {
                                            ClientAteFood?.Invoke();
                                        }
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
                                ClientDisconnected?.Invoke();
                            }
                        }, _cts.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Нормальное закрытие
                }
            }, _cts.Token);
        }

        private async Task SendAsync(GameState state)
        {
            if (_stream == null || !_connectedClient?.Connected == true)
                return;
            
            var head = state.Worm[0];
            var food = state.Food;
            var payload = $"{head.X},{head.Y},{(int)state.CurrentDirection},{state.Worm.Count},{food.X},{food.Y}";
            var data = Encoding.UTF8.GetBytes(payload);
            await _stream.WriteAsync(data, 0, data.Length);
        }

        /// <summary>
        /// Принудительно отключает подключенного клиента, сервер остается работать и ожидает новое подключение
        /// </summary>
        public async Task DisconnectAsync()
        {
            bool hadClient = _connectedClient != null;
            
            _cts?.Cancel();
            _timer?.Dispose();
            
            // Корректно закрываем соединение чтобы клиент получил уведомление об отключении
            if (_connectedClient != null && _connectedClient.Client != null)
            {
                _connectedClient.Client.Shutdown(SocketShutdown.Both);
                _connectedClient.Client.Close();
            }
            
            if (_stream != null) await _stream.DisposeAsync();
            _connectedClient?.Close();
            
            // ОБНУЛЯЕМ ТОЛЬКО ПОЛЯ КЛИЕНТА! СЛУШАТЕЛЬ ОСТАЕТСЯ РАБОТАТЬ
            _timer = null;
            _stream = null;
            _connectedClient = null;
            // ❗ СЕРВЕР НЕ ОСТАНАВЛИВАЕМ!
            // _listener?.Stop();
            // IsRunning = false;
            // _cts = null;
            // _listener = null;

            if (hadClient)
            {
                ClientDisconnected?.Invoke();
            }
        }

        public void Dispose() => DisconnectAsync().GetAwaiter().GetResult();
    }
}