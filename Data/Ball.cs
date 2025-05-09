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
    public class Ball : IBall
    {
        #region ctor

        public Ball(Vector initialPosition, Vector initialVelocity, double tableWidth, double tableHeight, double radius)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
            TableSize = new Vector(tableWidth, tableHeight);
            Radius = radius;
            Mass = new Random().NextDouble() * 5.0 + 0.5;
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

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        public void Move()
        {
            Vector velocity = (Vector)Velocity;
            Vector position = (Vector)Position;
            Position = new Vector(position.x + velocity.x, position.y + velocity.y);
            RaiseNewPositionChangeNotification();
        }

        #endregion private
    }
}