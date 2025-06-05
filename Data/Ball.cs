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
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor
        private static readonly Random random = new Random();
        private readonly DiagnosticLogger logger = DiagnosticLogger.Instance;
        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            position = initialPosition;
            velocity = initialVelocity;
            Mass = random.NextDouble() * 2.0 + 2.0;
            Running = true;
            MoveThread = null!;
  
        }
      
        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity
        {
            get
            {
                return velocity;
            }
        }
        public void SetVelocity(double x, double y)
        {
                velocity = new Vector(x, y);
        }

        public double Mass { get; }

        public IVector Position => position;

        #endregion IBall

        #region private
        private Vector position;
        private Vector velocity;

        private Thread? MoveThread;

        private volatile bool Running;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        private void Move(double deltaTime)
        {
            Vector velocity = (Vector)Velocity;
            position = new Vector(position.x + velocity.x * deltaTime, position.y + velocity.y * deltaTime);

            logger.Log(
                    eventType: 0,
                    ballId1: GetHashCode(),
                    mass1: Mass,
                    positionX1: position.x,
                    positionY1: position.y,
                    velocityX1: velocity.x,
                    velocityY1: velocity.y
                );

            RaiseNewPositionChangeNotification();
        }

        private void StartMoving()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            double lastUpdateTime = 0.0;
            while (Running)
            {
                double currentTime = stopwatch.Elapsed.TotalSeconds;
                double deltaTime = currentTime - lastUpdateTime;

                double speed = Math.Sqrt(Velocity.x * Velocity.x + Velocity.y * Velocity.y);
                double baseDelay = 1000.0 / (speed * 20.0 + 0.1);
                int delay = (int)Math.Clamp(baseDelay, 15, 40);

                if (deltaTime > 0.0)
                {
                    Move(deltaTime);
                    lastUpdateTime = currentTime;
                }

                Thread.Sleep(delay);
            }
        }
        #endregion private

        #region internal
        internal void Begin()
        {
            if (MoveThread == null || !MoveThread.IsAlive)
            {
                MoveThread = new Thread(new ThreadStart(StartMoving));
                MoveThread.Start();
            }
        }

        internal void Stop()
        {
            Running = false;
            MoveThread?.Join();
            MoveThread = null;
            DiagnosticLogger.Instance.Dispose();
        }
        #endregion internal
    }
}