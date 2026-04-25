// src/WormWinForms/GameState.cs
using System;
using System.Collections.Generic;

namespace WormWinForms.Model
{
    public enum Direction { Up = 0, Left = 1, Right = 2, Down = 3 }

    public sealed class GameState
    {
        public const int CellSize = 16;                 // пикселей
        public readonly Size FieldSize;                  // в ячейках
        public List<Square> Worm { get; } = new();
        public Square Food { get; private set; }
        public int Score { get; private set; }
        public int Lives { get; private set; } = 3;
        public Direction CurrentDirection { get; set; } = Direction.Right;
        public bool GameOver { get; private set; }

        public GameState(Size fieldSize)
        {
            FieldSize = new Size(
                Math.Max(1, fieldSize.Width),
                Math.Max(1, fieldSize.Height));
            Reset();
        }

        public void Reset()
        {
            Worm.Clear();
            Worm.Add(new Square(FieldSize.Width / 2, FieldSize.Height / 2));
            Score   = 0;
            Lives   = 3;
            GameOver = false;
            CurrentDirection = Direction.Right;
            SpawnFood();
        }

        public void Tick()
        {
            if (GameOver) return;

            MoveHead();
            CheckCollisions();
        }

        private void MoveHead()
        {
            var head = Worm[0];
            var newHead = CurrentDirection switch
            {
                Direction.Up    => new Square(head.X, head.Y - 1),
                Direction.Down  => new Square(head.X, head.Y + 1),
                Direction.Left  => new Square(head.X - 1, head.Y),
                Direction.Right => new Square(head.X + 1, head.Y),
                _ => head
            };
            Worm.Insert(0, newHead);
            Worm.RemoveAt(Worm.Count - 1); // обычный шаг – хвост отбрасывается
        }

        private void CheckCollisions()
        {
            var head = Worm[0];

            // Стены
            if (head.X < 0 || head.Y < 0 ||
                head.X >= FieldSize.Width || head.Y >= FieldSize.Height)
            {
                Die();
                return;
            }

            // Само‑съедение
            for (int i = 1; i < Worm.Count; i++)
                if (head.Equals(Worm[i]))
                { Die(); return; }

            // Пища
            if (head.Equals(Food))
            {
                Score++;
                Lives = Math.Min(Lives + 1, 10);
                Worm.Add(Worm[Worm.Count - 1]); // удлинить на один сегмент
                SpawnFood();
            }
        }

        private void Die()
        {
            Lives--;
            GameOver = Lives <= 0;
        }

        private void SpawnFood()
        {
            var rnd = new Random();
            Square candidate;
            do
            {
                candidate = new Square(
                    rnd.Next(FieldSize.Width),
                    rnd.Next(FieldSize.Height));
            } while (Worm.Exists(s => s.Equals(candidate)));

            Food = candidate;
        }
    }
}
