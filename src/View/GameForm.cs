using System;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;
using NAudio.Wave;
using SnakeNet.Model;
using SnakeNet.Input;
using SnakeNet.Network;

namespace SnakeNet.View
{
    public sealed partial class GameForm : Form
    {
        private readonly GameState _state = new();
        private readonly System.Windows.Forms.Timer _logicTimer = new() { Interval = 50 };   // 20 FPS
        private readonly System.Windows.Forms.Timer _drawTimer  = new() { Interval = 30 };   // 33 FPS
        private TextBox _txtServerIp;
        private Label _lblConnectionStatus;
        private TcpGameClient? _client;
        private TcpGameServer? _server;
        private Image _backgroundImage;
        private Mp3FileReader _mp3Reader;
        private WaveOutEvent _waveOut;
        private string _connectionStatus = string.Empty;
        private const int ServerPort = 12345;
        private bool _musicVolumeIncreasing = true;

        public GameForm()
        {
            InitializeComponent();                     // создаёт UI‑элементы
            
            // ✅ Самая главная ошибка в WinForms: без этого форма НИКОГДА не получит нажатия клавиш!
            this.KeyPreview = true;
            
            // ✅ Поле ввода IP адреса сервера
            _txtServerIp = new TextBox
            {
                Location = new Point(Config.FieldWidth * Config.CellSize + 20, 320),
                Size = new Size(100, 20),
                Text = "127.0.0.1",
                TabStop = false
            };
            Controls.Add(_txtServerIp);

            // ✅ Кнопка подключения
            _btnConnect.Location = new Point(Config.FieldWidth * Config.CellSize + 125, 318);
            _btnConnect.Size = new Size(75, 25);
            _btnConnect.Text = "Подключить";
            _btnConnect.UseVisualStyleBackColor = true;
            _btnConnect.TabStop = false;

            // ✅ Статус сетевого подключения
            _lblConnectionStatus = new Label
            {
                Location = new Point(Config.FieldWidth * Config.CellSize + 20, 350),
                Size = new Size(180, 20),
                ForeColor = Color.LightGray,
                Text = "Оффлайн",
                TabStop = false
            };
            Controls.Add(_lblConnectionStatus);
            
            // Сразу устанавливаем фокус на саму форму
            this.Focus();
            
            BindUi();
            
            // Загрузка фонового изображения
            try
            {
                _backgroundImage = Image.FromFile("Resources/background.png");
            }
            catch
            {
                _backgroundImage = null; // если файл отсутствует, используем стандартный цвет
            }

            _logicTimer.Tick += (s, e) => { _state.Tick(); };
            _drawTimer.Tick  += (s, e) => Invalidate(); // перерисовать форму
            
            // Динамическая скорость: переводим условные единицы в интервал таймера
            _state.SpeedChanged += () => 
            {
                // Чем выше скорость, тем меньше интервал между тиками
                _logicTimer.Interval = 1000 / (int)(5 + (_state.CurrentSpeed * 0.15));
            };
            
            // Устанавливаем начальную скорость
            _logicTimer.Interval = 1000 / (int)(5 + (_state.CurrentSpeed * 0.15));
            
            _logicTimer.Start();
            _drawTimer.Start();

            // ✅ Пробуем запустить сервер в фоновом режиме
            _server = new TcpGameServer(ServerPort);

            _server.ClientConnected += () =>
            {
                Invoke(() =>
                {
                    _lblConnectionStatus.Text = "🟢 Сервер! Клиент подключен";
                    _lblConnectionStatus.ForeColor = Color.LimeGreen;
                    _btnConnect.Enabled = true;
                    _btnConnect.Text = "Отключить";
                    _state.RemoteConnected = true;

                    // ❗ Запрещаем клиенту подключаться самому к себе!
                    // Закрываем клиентское соединение и игнорируем его
                    if (_client != null)
                    {
                        _client.Dispose();
                        _client = null;
                    }
                });
            };

            _server.ClientDisconnected += () =>
            {
                Invoke(() =>
                {
                    _lblConnectionStatus.Text = "🟢 Сервер! Ожидание клиента";
                    _lblConnectionStatus.ForeColor = Color.LimeGreen;
                    _btnConnect.Enabled = true;
                    _btnConnect.Text = "Подключить";
                    _state.RemoteConnected = false;
                });
            };

             _server.ClientPositionReceived += (x, y, dir, length) =>
             {
                 // Добавляем новую голову
                 _state.RemoteWorm.Insert(0, new Square(x, y));
                 
                 // Если червяк длиннее чем надо - обрезаем хвост
                 while (_state.RemoteWorm.Count > length)
                     _state.RemoteWorm.RemoveAt(_state.RemoteWorm.Count - 1);
                 
                 // Если длина выросла - удлиняем хвост дублируя последний сегмент
                 while (_state.RemoteWorm.Count < length)
                     _state.RemoteWorm.Add(_state.RemoteWorm[_state.RemoteWorm.Count - 1]);
             };



            _server.ClientAteFood += () =>
            {
                // Клиент съел еду - спавним новую глобально
                _state.SpawnFood();
            };
            
            try
            {
                _ = _server.ConnectAsync(_state).ContinueWith(task => 
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        Invoke(() => 
                        {
                            _btnConnect.Enabled = false;
                            _btnConnect.Text = "✓ Сервер";
                            _lblConnectionStatus.Text = "🟢 Сервер! Ожидание клиента";
                            _lblConnectionStatus.ForeColor = Color.LimeGreen;
                        });
                    }
                    else
                    {
                        _server = null;
                    }
                });
            }
            catch
            {
                // Порт уже занят - работаем как клиент
                _server = null;
            }

            _btnConnect.Click += async (s, e) => 
            {
                // Если есть подключенный клиент на сервере - отключаем его
                if (_server != null && _state.RemoteConnected && _client == null)
                {
                    await _server.DisconnectAsync();
                    return;
                }
            
                // Если уже подключены как клиент - отключаемся
                if (_client != null)
                {
                    await _client.DisconnectAsync();
                    _client.Dispose();
                    _client = null;
                    _state.RemoteConnected = false;
                    _state.RemoteWorm.Clear();
                    
                    _lblConnectionStatus.Text = "🔴 Отключено";
                    _lblConnectionStatus.ForeColor = Color.OrangeRed;
                    _btnConnect.Text = "Подключить";
                    _btnConnect.Enabled = true;
                    return;
                }

                var serverIp = _txtServerIp.Text.Trim();
                if (string.IsNullOrEmpty(serverIp)) serverIp = "127.0.0.1";
                
                // ❗ Запрещаем подключаться к самому себе ТОЛЬКО если к серверу еще никто не подключен
                if (_server != null && !_state.RemoteConnected && (serverIp == "127.0.0.1" || serverIp == "localhost" || serverIp == "0.0.0.0"))
                {
                    _lblConnectionStatus.Text = "❌ Нельзя подключаться к себе";
                    _lblConnectionStatus.ForeColor = Color.Orange;
                    _btnConnect.Text = "Подключить";
                    _btnConnect.Enabled = true;
                    return;
                }

                try
                {
                    _btnConnect.Enabled = false;
                    _btnConnect.Text = "⏳";
                    _client = new TcpGameClient(serverIp, ServerPort);
                    
                    await _client.ConnectAsync(_state);
                    
                    _lblConnectionStatus.Text = $"✅ Подключен к {serverIp}";
                    _lblConnectionStatus.ForeColor = Color.LimeGreen;
                    _btnConnect.Text = "Отключить";
                    _btnConnect.Enabled = true;
                    _state.RemoteConnected = true;

                    _client.ServerPositionReceived += (x, y, dir, length) =>
                    {
                        // Добавляем новую голову
                        _state.RemoteWorm.Insert(0, new Square(x, y));
                        
                        // Если червяк длиннее чем надо - обрезаем хвост
                        while (_state.RemoteWorm.Count > length)
                            _state.RemoteWorm.RemoveAt(_state.RemoteWorm.Count - 1);
                        
                        // Если длина выросла - удлиняем хвост дублируя последний сегмент
                        while (_state.RemoteWorm.Count < length)
                            _state.RemoteWorm.Add(_state.RemoteWorm[_state.RemoteWorm.Count - 1]);
                    };

                    _client.ServerDisconnected += () =>
                    {
                        Invoke(() =>
                        {
                            _lblConnectionStatus.Text = "🔴 Сервер отключился";
                            _lblConnectionStatus.ForeColor = Color.OrangeRed;
                            _btnConnect.Enabled = true;
                            _btnConnect.Text = "Подключить";
                            _state.RemoteConnected = false;
                            _state.RemoteWorm.Clear();
                            _client = null;
                        });
                    };

                    // Подписываемся на событие поедания еды клиентом чтобы отправить на сервер
                    _state.FoodEaten += () =>
                    {
                        _client?.NotifyFoodEaten();
                    };
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    _lblConnectionStatus.Text = "❌ Сервер недоступен";
                    _lblConnectionStatus.ForeColor = Color.Red;
                    _btnConnect.Text = "Подключить";
                    _btnConnect.Enabled = true;
                }
                catch (Exception ex)
                {
                    _lblConnectionStatus.Text = $"❌ {ex.Message}";
                    _lblConnectionStatus.ForeColor = Color.Red;
                    _btnConnect.Text = "Подключить";
                    _btnConnect.Enabled = true;
                }
            };

            // Фоновая музыка
            try
            {
                _mp3Reader = new Mp3FileReader("Resources/soundtrack.mp3");
                _waveOut = new WaveOutEvent();
            _waveOut.Init(_mp3Reader);
            _waveOut.PlaybackStopped += (s, e) =>
            {
                // Зацикливание трека только если музыка включена
                if (_waveOut != null && _waveOut.PlaybackState == PlaybackState.Stopped)
                {
                    _mp3Reader.Position = 0;
                    _waveOut.Play();
                }
            };
            // ❗ По умолчанию музыка НЕ играет
            // _waveOut.Play();
            }
            catch
            {
                // Если файл отсутствует или ошибка - просто не играем музыку
                _mp3Reader = null;
                _waveOut = null;
            }
        }

        private void BindUi()
        {
            _livesLabel.Text = $"❤ {_state.Lives}";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawField(e.Graphics);
        }

        private void DrawField(Graphics g)
        {
            // Очищаем всё окно
            g.Clear(Color.Black);
            
            // ✅ Реальный размер игрового поля в пикселях
            int fieldWidth  = Config.FieldWidth  * Config.CellSize;
            int fieldHeight = Config.FieldHeight * Config.CellSize;
            
            // Рисуем фон только в границах игрового поля
            if (_backgroundImage != null)
            {
                g.DrawImage(_backgroundImage, 0, 0, fieldWidth, fieldHeight);
            }
            else
            {
                g.FillRectangle(Brushes.SteelBlue, 0, 0, fieldWidth, fieldHeight);
            }

            // еда
            var f = _state.Food;
            g.FillEllipse(Brushes.Crimson,
                f.X * Config.CellSize + 2,
                f.Y * Config.CellSize + 2,
                Config.CellSize - 4, Config.CellSize - 4);

            // змейка
            for (int i = 0; i < _state.Worm.Count; i++)
            {
                var part = _state.Worm[i];
                var brush = i == 0 ? Brushes.ForestGreen : Brushes.GreenYellow;
                g.FillRectangle(brush,
                    part.X * Config.CellSize + 1,
                    part.Y * Config.CellSize + 1,
                    Config.CellSize - 2, Config.CellSize - 2);
            }

            // Удаленная змейка
            if (_state.RemoteConnected)
            {
                for (int i = 0; i < _state.RemoteWorm.Count; i++)
                {
                    var part = _state.RemoteWorm[i];
                    var brush = i == 0 ? Brushes.OrangeRed : Brushes.Gold;
                    g.FillRectangle(brush,
                        part.X * Config.CellSize + 1,
                        part.Y * Config.CellSize + 1,
                        Config.CellSize - 2, Config.CellSize - 2);
                }
            }

            // UI‑индикаторы
            _livesLabel.Text = $"❤ {_state.Lives}";
            _scoreLabel2.Text = _state.Score.ToString("D4");
            
            // ✅ Подсказки управления справа от игрового поля (ВНИЗУ)
            int helpX = Config.FieldWidth * Config.CellSize + 20;
            int helpStartY = 380; // Начинаем рисовать подсказки ниже сетевых элементов
            
            using var helpFont = new Font("Consolas", 8f); // Уменьшили шрифт
            var grayBrush = Brushes.LightGray; // Сделали цвет менее ярким
            
            g.DrawString("УПРАВЛЕНИЕ:", helpFont, grayBrush, helpX, helpStartY);
            g.DrawString("W / ↑  Вверх", helpFont, grayBrush, helpX, helpStartY + 20);
            g.DrawString("S / ↓  Вниз", helpFont, grayBrush, helpX, helpStartY + 35);
            g.DrawString("A / ←  Влево", helpFont, grayBrush, helpX, helpStartY + 50);
            g.DrawString("D / →  Вправо", helpFont, grayBrush, helpX, helpStartY + 65);
            g.DrawString("ESC  Выход", helpFont, grayBrush, helpX, helpStartY + 85);
            g.DrawString("Пробел  Рестарт", helpFont, grayBrush, helpX, helpStartY + 100);
            g.DrawString($"F9      Бессмертие {(_state.UnlimitedLives ? "✅ ВКЛ" : "❌ ВЫКЛ")}", helpFont, grayBrush, helpX, helpStartY + 115);
            g.DrawString($"M       Громкость {(_waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing ? (int)(_waveOut.Volume * 100) : 0)}%", helpFont, grayBrush, helpX, helpStartY + 130);
            g.DrawString("Каждые 5 еды +1 жизнь", helpFont, grayBrush, helpX, helpStartY + 145);
            g.DrawString($"Скорость: {_state.CurrentSpeed}", helpFont, grayBrush, helpX, helpStartY + 160);
            
            // Статус подключения
            if (!string.IsNullOrEmpty(_connectionStatus))
            {
                g.DrawString(_connectionStatus, helpFont, Brushes.Red, helpX, 300);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            Keyboard.ChangeState(e.KeyCode, true);
            
            // Выход из игры по клавише Esc
            if (e.KeyCode == Keys.Escape)
            {
                Close();
                return;
            }
            
            // При Game Over по нажатию Пробел перезапускаем игру
            if (_state.GameOver && e.KeyCode == Keys.Space)
            {
                _state.Reset();
                return;
            }
            
            // Запрет на поворот на 180 градусов
            switch (e.KeyCode)
            {
                case Keys.W:
                case Keys.Up:
                    if (_state.CurrentDirection != Direction.Down)
                        _state.CurrentDirection = Direction.Up;    
                    break;
                case Keys.S:
                case Keys.Down:
                    if (_state.CurrentDirection != Direction.Up)
                        _state.CurrentDirection = Direction.Down;  
                    break;
                case Keys.A:
                case Keys.Left:
                    if (_state.CurrentDirection != Direction.Right)
                        _state.CurrentDirection = Direction.Left;  
                    break;
            case Keys.D:
            case Keys.Right:
                if (_state.CurrentDirection != Direction.Left)
                    _state.CurrentDirection = Direction.Right; 
                break;
                
            // Клавиша F9 - переключение бесконечных жизней (для тестирования)
            case Keys.F9:
                _state.UnlimitedLives = !_state.UnlimitedLives;
                break;
            }

            // Изменение громкости музыки по клавише M (шаг 25%)
            if (e.KeyCode == Keys.M && _waveOut != null)
            {
                // Запускаем музыку если она еще не играет
                if (_waveOut.PlaybackState != PlaybackState.Playing)
                {
                    _mp3Reader.Position = 0;
                    _waveOut.Play();
                    _waveOut.Volume = 0;
                    _musicVolumeIncreasing = true;
                }

                // Текущая громкость от 0.0 до 1.0 округляем до 0, 0.25, 0.5, 0.75, 1.0
                float currentVolume = (float)Math.Round(_waveOut.Volume * 4) / 4;

                if (_musicVolumeIncreasing)
                {
                    _waveOut.Volume = Math.Min(currentVolume + 0.25f, 1.0f);
                    if (Math.Abs(_waveOut.Volume - 1.0f) < 0.01f)
                        _musicVolumeIncreasing = false;
                }
                else
                {
                    _waveOut.Volume = Math.Max(currentVolume - 0.25f, 0f);
                    if (Math.Abs(_waveOut.Volume - 0f) < 0.01f)
                        _musicVolumeIncreasing = true;
                }
            }

            // ✅ Обязательно помечаем событие как обработанное!
            // Без этого Windows Forms отправляет его дальше несуществующим кнопкам
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            Keyboard.ChangeState(e.KeyCode, false);
        }

        // ✅ Исправляем проблему с фокусом на стрелки!
        // В WinForms по умолчанию стрелки ←↑→↓ используются для навигации по контролам
        // Переопределяем этот метод чтобы стрелки всегда передавались в OnKeyDown формы
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Left || keyData == Keys.Right || 
                keyData == Keys.Up || keyData == Keys.Down ||
                keyData == Keys.Space)
            {
                // ✅ Говорим что мы обработали эту клавишу - не передаём дальше контролам
                // Это полностью запрещает переключение фокуса на кнопки и другие элементы
                OnKeyDown(new KeyEventArgs(keyData));
                return true;
            }
            
            return base.ProcessDialogKey(keyData);
        }

        // ✅ Дополнительно запрещаем всем контролам получать фокус по умолчанию
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // Отключаем фокус для ВСЕХ контрол на форме
            foreach (Control control in Controls)
            {
                control.GotFocus += (s, ev) => 
                {
                    // Как только какой-то контрол получил фокус - сразу возвращаем его обратно форме
                    this.Focus();
                };
            }
            
            // Сразу после загрузки формы устанавливаем фокус на неё
            this.ActiveControl = null;
            this.Focus();
        }
    }
}
