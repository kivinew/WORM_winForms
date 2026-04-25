// src/WormWinForms/GameForm.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using WormWinForms.Model;
using WormWinForms.Input;

namespace WormWinForms
{
    public sealed partial class GameForm : Form
    {
        private readonly GameState _state;
        private readonly Timer _logicTimer = new() { Interval = 50 }; // 20 FPS
        private readonly Timer _renderTimer = new() { Interval = 30 }; // 33 FPS
        private readonly TcpGameClient _client;

        public GameForm()
        {
            InitializeComponent();
            _state = new GameState(new Size(20, 20));
            BindUi();

            _logicTimer.Tick   += (s, e) => { _state.Tick(); Refresh(); };
            _renderTimer.Tick  += (s, e) => { Invalidate(); };
            _logicTimer.Start();
            _renderTimer.Start();

            // сетевой клиент (по желанию)
            _client = new TcpGameClient("127.0.0.1", 12345);
            _btnConnect.Click += (s, e) => _client.ConnectAsync(_state);
        }

        private void BindUi()
        {
            _lifeBar.Maximum = 10;
            _lifeBar.Value   = _state.Lives;
            _speedBar.Maximum = 100;
            _speedBar.Value   = 60;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawField(e.Graphics);
        }

        private void DrawField(Graphics g)
        {
            // фон
            g.Clear(Color.SteelBlue);

            // еда
            var f = _state.Food;
            g.FillEllipse(Brushes.Red,
                f.X * GameState.CellSize,
                f.Y * GameState.CellSize,
                GameState.CellSize, GameState.CellSize);

            // змейка
            for (int i = 0; i < _state.Worm.Count; i++)
            {
                var part = _state.Worm[i];
                var brush = i == 0 ? Brushes.DarkGreen : Brushes.LimeGreen;
                g.FillRectangle(brush,
                    part.X * GameState.CellSize,
                    part.Y * GameState.CellSize,
                    GameState.CellSize, GameState.CellSize);
            }

            // UI‑индикаторы
            _lifeBar.Value   = _state.Lives;
            _speedBar.Value  = (int)(_state.Score % 100) + 60; // простой пример
            _scoreLabel2.Text = _state.Score.ToString("D4");
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            Input.ChangeState(e.KeyCode, true);
            switch (e.KeyCode)
            {
                case Keys.W: _state.CurrentDirection = Direction.Up;    break;
                case Keys.S: _state.CurrentDirection = Direction.Down;  break;
                case Keys.A: _state.CurrentDirection = Direction.Left;  break;
                case Keys.D: _state.CurrentDirection = Direction.Right; break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            Input.ChangeState(e.KeyCode, false);
        }
    }
}
