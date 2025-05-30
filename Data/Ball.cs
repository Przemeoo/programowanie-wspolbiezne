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
        private readonly DiagnosticLogger logger = DiagnosticLogger.Instance;
        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            _position = initialPosition;
            _velocity = initialVelocity;
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
                return _velocity;
            }
            set
            {
                _velocity = (Vector)value;
            }
        }

        public double Mass { get; }

        public IVector Position => _position;

        #endregion IBall

        #region private
        private Vector _position;
        private Vector _velocity;

        private Thread? MoveThread;

        private volatile bool Running;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        private void Move()
        {
            Vector velocity = (Vector)Velocity;
            _position = new Vector(_position.x + velocity.x, _position.y + velocity.y);
            logger.Log($"Ball ID: {GetHashCode()}, Mass:{Mass}, Position: ({_position.x}, {_position.y}), Velocity: ({velocity.x}, {velocity.y})");
            RaiseNewPositionChangeNotification();
        }

        private void StartMoving()
        {
            while (Running)
            {
                Move();
                double speed = Math.Sqrt(Velocity.x * Velocity.x + Velocity.y * Velocity.y);
                double baseDelay = 1000.0 / (speed * 20.0 + 0.1);
                int delay = (int)Math.Clamp(baseDelay, 15, 40);
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