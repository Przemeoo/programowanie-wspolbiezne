//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor
        private static readonly Random random = new Random();

        internal Ball(Vector initialPosition, Vector initialVelocity, double tableWidth, double tableHeight, double radius)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
            TableSize = new Vector(tableWidth, tableHeight);
            Radius = radius;
            Mass = random.NextDouble() * 5.0 + 0.5;
            Running = true;
            MoveThread = null!;
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity { get; set; }

        public double Radius { get; }

        public double Mass { get; }

        public IVector Position { get; set; }

        public IVector TableSize { get; }

        #endregion IBall

        #region private

        private Thread? MoveThread;

        private volatile bool Running;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        private void Move()
        {
            Vector velocity = (Vector)Velocity;
            Vector position = (Vector)Position;
            Position = new Vector(position.x + velocity.x, position.y + velocity.y);
            RaiseNewPositionChangeNotification();
        }

        private void StartMoving()
        {
            while (Running)
            {
                Move(); 
                Thread.Sleep(20);
            }
        }

        internal void Begin()
        {
            if (MoveThread == null || !MoveThread.IsAlive)
            {
                MoveThread = new Thread(new ThreadStart(StartMoving)); 
                MoveThread.Start(); 
            }
        }

        internal void Dispose()
        {
            Running = false;
            MoveThread?.Join();
            MoveThread = null;
        }

        #endregion private
    }
}