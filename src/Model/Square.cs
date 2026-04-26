namespace SnakeNet.Model
{
    public sealed class Square
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Square(int x, int y) { X = x; Y = y; }

        public override bool Equals(object? obj) =>
            obj is Square s && s.X == X && s.Y == Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);
    }
}
