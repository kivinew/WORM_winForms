using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace SnakeNet.Model
{
    public sealed class GameState
    {
        public Size FieldSize { get; }
        public List<Square> Worm { get; } = new();
        public List<Square> RemoteWorm { get; } = new();
        public bool RemoteConnected { get; set; }
        public Square Food { get; set; }
        public int Score { get; private set; }
        public int Lives { get; private set; } = Config.InitLives;
        public bool UnlimitedLives { get; set; } = false;
        public Direction CurrentDirection { get; set; } = Direction.Right;
        public int CurrentSpeed { get; private set; } = (int)(Config.InitSpeed * 0.7);
        public event Action? FoodEaten;
        public event Action? SpeedChanged;
        public bool GameOver { get; private set; }

        private readonly Random _rnd = new();
        private int _skipTicks = 0;
        private int _foodCounter = 0;
        private System.Threading.Timer? _speedResetTimer;
        public GameState()
        {
            FieldSize = new Size(Config.FieldWidth, Config.FieldHeight);
            Reset();
        }

        public void Reset()
        {
            Worm.Clear();
            RemoteWorm.Clear();
            
            // Случайное место спавна на карте
            int spawnX = _rnd.Next(FieldSize.Width);
            int spawnY = _rnd.Next(FieldSize.Height);
            Worm.Add(new Square(spawnX, spawnY));
            
            // Определяем направление движения к центру карты
            int centerX = FieldSize.Width / 2;
            int centerY = FieldSize.Height / 2;
            
            if (Math.Abs(spawnX - centerX) > Math.Abs(spawnY - centerY))
            {
                // Если по X дальше - двигаемся по горизонтали
                CurrentDirection = spawnX < centerX ? Direction.Right : Direction.Left;
            }
            else
            {
                // Если по Y дальше - двигаемся по вертикали
                CurrentDirection = spawnY < centerY ? Direction.Down : Direction.Up;
            }

            Score = 0;
            Lives = Config.InitLives;
            UnlimitedLives = false;
            CurrentSpeed = (int)(Config.InitSpeed * 0.7);
            GameOver = false;
            _foodCounter = 0;
            SpawnFood();
            SpeedChanged?.Invoke();
        }

        public void Tick()
        {
            if (GameOver) return;
            
            if (_skipTicks > 0)
            {
                _skipTicks--;
                return;
            }
            
            MoveHead();
            CheckCollisions();
        }

        private void MoveHead()
        {
            var head = Worm[0];
            var next = CurrentDirection switch
            {
                Direction.Up    => new Square(head.X, head.Y - 1),
                Direction.Down  => new Square(head.X, head.Y + 1),
                Direction.Left  => new Square(head.X - 1, head.Y),
                Direction.Right => new Square(head.X + 1, head.Y),
                _ => head
            };
            Worm.Insert(0, next);
            Worm.RemoveAt(Worm.Count - 1); // обычный шаг – хвост отбрасывается
        }

        private void CheckCollisions()
        {
            var head = Worm[0];

            // Стены
            if (head.X < 0 || head.Y < 0 ||
                head.X >= FieldSize.Width || head.Y >= FieldSize.Height)
            {
                Die(); return;
            }

            // Само‑съедение
            for (int i = 1; i < Worm.Count; i++)
                if (head.Equals(Worm[i])) { Die(); return; }

            // Столкновение с другим червяком
            if (RemoteConnected)
            {
                for (int i = 0; i < RemoteWorm.Count; i++)
                    if (head.Equals(RemoteWorm[i])) { Die(); return; }
            }

            // Еда
            if (head.Equals(Food))
            {
                Score++;
                _foodCounter++;
                FoodEaten?.Invoke();
                
                // Дополнительная жизнь каждые 5 съеденных единиц еды
                if (_foodCounter % 5 == 0)
                {
                    Lives = Math.Min(Lives + 1, Config.InitLives);
                }
                
                // Увеличиваем скорость на 5%
                CurrentSpeed = Math.Min((int)(CurrentSpeed * 1.05), Config.MaxSpeed);
                SpeedChanged?.Invoke();
                
                Worm.Add(Worm[Worm.Count - 1]); // удлиняем на один сегмент
                SpawnFood();
            }
        }

        private void Die()
        {
            if (!UnlimitedLives)
                Lives--;
            
            if (Lives <= 0 && !UnlimitedLives)
            {
                GameOver = true;
            }
            else
            {
                // Если остались жизни - спавним змейку в случайном месте, движется к центру
                Worm.Clear();
                
                int spawnX = _rnd.Next(FieldSize.Width);
                int spawnY = _rnd.Next(FieldSize.Height);
                Worm.Add(new Square(spawnX, spawnY));

                int centerX = FieldSize.Width / 2;
                int centerY = FieldSize.Height / 2;
                
                if (Math.Abs(spawnX - centerX) > Math.Abs(spawnY - centerY))
                {
                    CurrentDirection = spawnX < centerX ? Direction.Right : Direction.Left;
                }
                else
                {
                    CurrentDirection = spawnY < centerY ? Direction.Down : Direction.Up;
                }
                
                // Пропускаем 6 игровых тиков (0.3 секунды)
                // НЕ БЛОКИРУЕМ ПОТОК! UI и клавиши продолжают работать
                _skipTicks = 6;
                
                // Сбрасываем скорость через 5 секунд после спавна
                _speedResetTimer?.Dispose();
                var uiContext = SynchronizationContext.Current;
                _speedResetTimer = new System.Threading.Timer(_ => 
                {
                    uiContext.Post(__ => 
                    {
                        CurrentSpeed = (int)(Config.InitSpeed * 0.7);
                        SpeedChanged?.Invoke();
                        _speedResetTimer?.Dispose();
                    }, null);
                }, null, 5000, Timeout.Infinite);
            }
        }

        public void SpawnFood()
        {
            // Если змейка заняла ВСЁ поле - закончить игру
            if (Worm.Count >= FieldSize.Width * FieldSize.Height)
            {
                GameOver = true;
                return;
            }
            
            Square candidate;
            do
            {
                candidate = new Square(
                    _rnd.Next(FieldSize.Width),
                    _rnd.Next(FieldSize.Height));
            } while (Worm.Exists(s => s.Equals(candidate)));

            Food = candidate;
        }
    }
}
