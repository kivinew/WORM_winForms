using NUnit.Framework;
using System.Drawing;
using SnakeNet.Model;

namespace SnakeNet.Tests
{
    public class GameStateTests
    {
        [Test]
        public void NewGame_HasOneSegment()
        {
            var gs = new GameState();
            Assert.AreEqual(1, gs.Worm.Count);
        }

        [Test]
        public void Move_ChangesHeadAccordingDirection()
        {
            var gs = new GameState();
            var start = gs.Worm[0];
            gs.CurrentDirection = Direction.Up;
            gs.Tick();
            Assert.AreEqual(start.X, gs.Worm[0].X);
            Assert.AreEqual(start.Y - 1, gs.Worm[0].Y);
        }

        [Test]
        public void EatFood_IncreasesScoreAndLength()
        {
            var gs = new GameState();
            gs.Food = new Square(gs.Worm[0].X, gs.Worm[0].Y); // еда на голове
            gs.Tick(); // обработка еды
            Assert.AreEqual(1, gs.Score);
            Assert.AreEqual(2, gs.Worm.Count);
        }
    }
}
