using System;

namespace SnakeNet.Model
{
    public static class Config
    {
        public const int CellSize   = 24;      // px ✅ Увеличен масштаб интерфейса на 50%
        public const int FieldWidth = 20;      // ячеек
        public const int FieldHeight= 20;
        public const int InitLives  = 3;
        public const int MaxSpeed   = 100;    // условные единицы
        public const int InitSpeed  = 60;
    }
}
