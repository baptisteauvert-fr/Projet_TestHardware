using System;
using System.Collections.Generic;
using UnityEngine;

namespace SnakeArcade
{
    public enum SnakeState { Ready, Playing, GameOver, Won }

    // Grid rules are independent of rendering, frame rate and XR devices.
    public sealed class SnakeGameModel
    {
        readonly List<Vector2Int> body = new List<Vector2Int>();
        readonly System.Random random;
        Vector2Int direction, queuedDirection;
        bool directionQueued;
        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<Vector2Int> Body => body;
        public Vector2Int Food { get; private set; }
        public SnakeState State { get; private set; }
        public int Score { get; private set; }
        public Vector2Int Direction => direction;
        public float StepInterval => Mathf.Max(0.095f, 0.23f - Score * 0.004f);

        public SnakeGameModel(int width = 20, int height = 20, int seed = 0)
        {
            if (width < 8 || height < 8) throw new ArgumentOutOfRangeException(nameof(width));
            Width = width; Height = height;
            random = new System.Random(seed);
            ResetBody();
            State = SnakeState.Ready;
        }

        void ResetBody()
        {
            body.Clear();
            for (int i = 0; i < 4; i++) body.Add(new Vector2Int(Width / 2 - i, Height / 2));
            direction = queuedDirection = Vector2Int.right;
            directionQueued = false;
            Score = 0;
            SpawnFood();
        }

        public void StartRound()
        {
            ResetBody();
            State = SnakeState.Playing;
        }

        public bool QueueDirection(Vector2Int next)
        {
            if (State != SnakeState.Playing || directionQueued || Mathf.Abs(next.x) + Mathf.Abs(next.y) != 1)
                return false;
            if (next == -direction || next == direction) return false;
            queuedDirection = next;
            directionQueued = true;
            return true;
        }

        // Returns true only when the snake advances. A tail cell is free on non-eating moves.
        public bool Tick()
        {
            if (State != SnakeState.Playing) return false;
            if (directionQueued) direction = queuedDirection;
            directionQueued = false;
            Vector2Int next = body[0] + direction;
            bool eating = next == Food;
            int occupied = body.Count - (eating ? 0 : 1);
            if (next.x < 0 || next.x >= Width || next.y < 0 || next.y >= Height)
            { State = SnakeState.GameOver; return false; }
            for (int i = 0; i < occupied; i++)
                if (body[i] == next) { State = SnakeState.GameOver; return false; }
            body.Insert(0, next);
            if (eating)
            {
                Score++;
                if (body.Count == Width * Height) State = SnakeState.Won;
                else SpawnFood();
            }
            else body.RemoveAt(body.Count - 1);
            return true;
        }

        void SpawnFood()
        {
            int freeIndex = random.Next(Width * Height - body.Count);
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if (body.Contains(cell)) continue;
                    if (freeIndex-- == 0) { Food = cell; return; }
                }
        }
    }
}
