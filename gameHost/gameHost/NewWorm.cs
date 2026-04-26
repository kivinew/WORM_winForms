using System;
using System.Collections.Generic;
using Worm_WinForms;

namespace gameHost
{
    public partial class WormServer
    {
        public class NewWorm
        {
            private int Direction;          // направление движения

            private NewWorm()               // конструктор червя
            {
                var worm = new List<Square>();
                for (var i = 0; i < 3; i++)
                {
                    worm.Add(new Square(new Random().Next(), new Random().Next()));
                }
                Direction = new Random().Next(0, 3); // случайное направление движения
            }

        }
    }
}