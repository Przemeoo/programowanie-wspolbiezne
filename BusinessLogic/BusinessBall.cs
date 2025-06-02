//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        public Ball(Data.IBall ball, double tableWidth, double tableHeight, double radius, IDiagnosticLogger underneathLogger)
        {
            _radius = radius;
            dataBall = ball;
            _tableWidth = tableWidth;
            _tableHeight = tableHeight;
            logger = underneathLogger;
            dataBall.NewPositionNotification += RaisePositionChangeEvent;
        }
        #region IBall

        public event EventHandler<IPosition>? NewPositionNotification;
        public double Mass => dataBall.Mass;
        public double Radius => _radius;
        #endregion IBall

        #region internal
        internal void WallCollision()
        {
            double borderMargin = 8.0;
            IVector velocity = dataBall.Velocity;
            IVector newPosition = dataBall.Position;

            if (newPosition.x <= 0 && velocity.x < 0)
            {
                dataBall.SetVelocity(-velocity.x, velocity.y);
                logger.Log($"Wall collision (left) for Ball ID: {GetHashCode()}, Position: ({newPosition.x}, {newPosition.y}), New Velocity: ({-velocity.x}, {velocity.y})");
            }
            else if (newPosition.x + Radius * 2 >= _tableWidth - borderMargin && velocity.x > 0)
            {
                dataBall.SetVelocity(-velocity.x, velocity.y);
                logger.Log($"Wall collision (right) for Ball ID: {GetHashCode()}, Position: ({newPosition.x}, {newPosition.y}), New Velocity: ({-velocity.x}, {velocity.y})");
            }
            if (newPosition.y <= 0 && velocity.y < 0)
            {
                dataBall.SetVelocity(velocity.x, -velocity.y);
                logger.Log($"Wall collision (top) for Ball ID: {GetHashCode()}, Position: ({newPosition.x}, {newPosition.y}), New Velocity: ({velocity.x}, {-velocity.y})");
            }
            else if (newPosition.y + Radius * 2 >= _tableHeight - borderMargin && velocity.y > 0)
            {
                dataBall.SetVelocity(velocity.x, -velocity.y);
                logger.Log($"Wall collision (bottom) for Ball ID: {GetHashCode()}, Position: ({newPosition.x}, {newPosition.y}), New Velocity: ({velocity.x}, {-velocity.y})");
            }
        }

        internal void BallsCollision(Ball otherBall)
        {
            if (otherBall == this)
                return;

            IVector position1 = dataBall.Position;
            IVector v1 = dataBall.Velocity;
            IVector position2 = otherBall.dataBall.Position;
            IVector v2 = otherBall.dataBall.Velocity;

            double x1 = position1.x + Radius;
            double y1 = position1.y + Radius;
            double x2 = position2.x + otherBall.Radius;
            double y2 = position2.y + otherBall.Radius;
            double dx = x1 - x2;
            double dy = y1 - y2;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance >= Radius + otherBall.Radius || distance < 1e-10)
                return;

            double dvx = v1.x - v2.x, dvy = v1.y - v2.y;
            double dot = dx * dvx + dy * dvy;
            if (dot >= 0)
                return;

            double m1 = dataBall.Mass, m2 = otherBall.dataBall.Mass;
            double factor = 2 * dot / (distance * distance * (m1 + m2));
            dataBall.SetVelocity(v1.x - factor * m2 * dx, v1.y - factor * m2 * dy); 
            otherBall.dataBall.SetVelocity(v2.x + factor * m1 * dx, v2.y + factor * m1 * dy);

            logger.Log($"Balls collision between Ball1 ID: {GetHashCode()}, Position: ({x1}, {y1}), New Velocity: ({dataBall.Velocity.x}, {dataBall.Velocity.y}) and Ball2 ID: {otherBall.GetHashCode()}, Position: ({x2}, {y2}), New Velocity: ({otherBall.dataBall.Velocity.x}, {otherBall.dataBall.Velocity.y})");
        }

        #endregion internal

        #region private

        public readonly Data.IBall dataBall;

        private readonly double _radius;

        private readonly double _tableWidth;

        private readonly double _tableHeight;

        private readonly IDiagnosticLogger logger;

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
        }

        #endregion private
    }
}