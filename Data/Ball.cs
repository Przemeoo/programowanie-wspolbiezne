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

        internal Ball(Vector initialPosition, Vector initialVelocity, double tableWidth, double tableHeight, double radius)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
            Radius = radius;
            Mass = new Random().NextDouble() * 5.0 + 0.5;
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity { get; set; }

        public double Radius { get; }

        public double Mass { get; }

        public IVector GetPosition()
        {
            return Position;
        }

        public void MoveTo(IVector newPosition)
        {
            Position = (Vector)newPosition;
            RaiseNewPositionChangeNotification();
        }

        #endregion IBall

        #region private

        private Vector Position;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        internal void Move()
        {
            Vector velocity = (Vector)Velocity;
            Position = new Vector(Position.x + velocity.x, Position.y + velocity.y);
            RaiseNewPositionChangeNotification();
        }

        #endregion private
    }
}
