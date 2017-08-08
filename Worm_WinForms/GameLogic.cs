using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Worm_WinForms
{
    public partial class WormGame
    {
        private NetworkStream _stream;
        private TcpClient _client;

        #region Игровая логика
        private void UpdateWorm(object sender, EventArgs eventArgs)
        {
            if (_gameover) return;
            for (var i = _worm.Count - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    switch (Direction)
                    {
                        case 0:
                            _worm[0].Y--;
                            break;
                        case 1:
                            _worm[0].X--;
                            break;
                        case 2:
                            _worm[0].X++;
                            break;
                        case 3:
                            _worm[0].Y++;
                            break;
                    }
                    // проверка на выход за границы

                    var head = _worm[0];
                    if (head.X > _fieldSize.Width || head.X < 0 || head.Y > _fieldSize.Height || head.Y < 0)
                        Death();
                    // проверка на столкновение с элементами тела червя
                    for (var j = 1; j < _worm.Count; j++)
                        if ((head.X == _worm[j].X) && (head.Y == _worm[j].Y))
                            Death();
                    // проверка на столкновение с призами
                    if ((head.X != _food.X) || (head.Y != _food.Y)) continue;
                    
                    _score++;
                    _speedBar.Value++;
                    if (_speedBar.Value == Constants.MaxSpeed)
                    {
                        _speedBar.Value = (int)Constants.NormSpeed;
                        _lifeBar.Value++;
                    }
                    if (_score % 50 == 0) // даем одну жизнь при наборе очередных 50 очков
                    {
                        _lifeBar.Value++;
                    }
                    _wormLoop.Interval = (int)(1000 / (_speedBar.Value / 10.0f));
                }
                else
                {
                    _worm[i].X = _worm[i - 1].X;
                    _worm[i].Y = _worm[i - 1].Y;
                }
            }
        }

        private void StartGame(bool newGame)
        {
            if (newGame)
            {
                _worm.Clear();
                var head = new Square(10, 8);
                _worm.Add(head);
                //GenerateFood();
                _gameover = false;
                _score = 0;
                _lifeBar.Value = 3;
                _speedBar.Value = (int)Constants.NormSpeed;
                _wormLoop.Interval = (int)(1000 / (_speedBar.Value / 10.0f));
            }
            else
            {
                _gameover = false;
                _worm.Reverse(); // разворачиваем тело червя
                Direction = 3 - Direction; // червь направляется в противоположную сторону
            }
        }

        private void Death()
        {
            _lifeBar.Value--;
            _gameover = true;
        }
        #endregion

        #region Сетевое взаимодействие
        public void ConnectToServer()
        {
            try
            {
                _client = new TcpClient("127.0.0.1", 12345);
                var dataString = _worm[0]+Direction.ToString();
                var data = Encoding.UTF8.GetBytes(dataString);
                _stream = _client.GetStream();
                _stream.Write(data, 0, data.Length);
                MessageBox.Show(Encoding.Default.GetString(data));
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Данные от сервера не получены!" + ex);
            }
        }
        #endregion
    }
}