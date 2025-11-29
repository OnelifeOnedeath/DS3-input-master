using System;

namespace DS3InputMaster.Core.Emulation
{
    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        // Статические методы
        public static Vector2 Zero => new Vector2(0, 0);
        public static Vector2 One => new Vector2(1, 1);

        // Методы для операций
        public Vector2 Normalized()
        {
            var length = (float)Math.Sqrt(X * X + Y * Y);
            if (length > 0)
                return new Vector2(X / length, Y / length);
            return this;
        }

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 a, float b) => new Vector2(a.X * b, a.Y * b);
        public static Vector2 operator /(Vector2 a, float b) => new Vector2(a.X / b, a.Y / b);
    }
}
