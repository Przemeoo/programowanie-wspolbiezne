//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

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
          
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity { get; set; }

        #endregion IBall

        #region public

        public double Radius { get; }

        #endregion public

        #region private

        private Vector Position;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        internal void Move(Vector tableSize)
        {
            Vector velocity = (Vector)Velocity;
            Position = new Vector(Position.x + velocity.x, Position.y + velocity.y);

            // Kolizja z lewą/prawą
            if (Position.x - Radius < 0)
            {
                Position = new Vector(Radius, Position.y);
                velocity = new Vector(-velocity.x, velocity.y);
            }
            else if (Position.x + Radius > tableSize.x)
            {
                Position = new Vector(tableSize.x - Radius, Position.y);
                velocity = new Vector(-velocity.x, velocity.y);
            }

            // Kolizja z górą/dołem
            if (Position.y - Radius < 0)
            {
                Position = new Vector(Position.x, Radius);
                velocity = new Vector(velocity.x, -velocity.y);
            }
            else if (Position.y + Radius > tableSize.y)
            {
                Position = new Vector(Position.x, tableSize.y - Radius);
                velocity = new Vector(velocity.x, -velocity.y);
            }

            Velocity = velocity;
            RaiseNewPositionChangeNotification();
        }


        #endregion private
    }
}
