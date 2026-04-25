// src/WormWinForms.Tests/GameStateTests.cs
using System.Drawing;
using NUnit.Framework;
using WormWinForms.Model;

namespace WormWinForms.Tests
{
    public class GameStateTests
    {
        [Test]
        public void NewGame_HasOneSegment_AndFoodNotOnWorm()
        {
            var gs = new GameState(new Size(10, 10));
            Assert.That(gs.Worm.Count, Is.EqualTo(1));
            Assert.That(gs.Worm[0], Is.Not.EqualTo(gs.Food));
        }

        [Test]
        public void Move_ChangesHeadPosition_AccordingDirection()
        {
            var gs = new GameState(new Size(5, 5));
            var start = gs.Worm[0];
            gs.CurrentDirection = Direction.Up;
            gs.Tick();
            Assert.That(gs.Worm[0].X, Is.EqualTo(start.X));
            Assert.That(gs.Worm[0].Y, Is.EqualTo(start.Y - 1));
        }

        // …другие тесты: столкновения, рост, жизнь и т.д.
    }
}
