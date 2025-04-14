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
            Vector correctedTableSize = new Vector(tableSize.x - 4, tableSize.y - 4);

            Vector velocity = (Vector)Velocity;
            Position = new Vector(Position.x + velocity.x, Position.y + velocity.y);

            if (Position.x < 0)
            {
                Position = new Vector(0, Position.y);
                velocity = new Vector(-velocity.x, velocity.y);
            }
            else if (Position.x + Radius * 2 > correctedTableSize.x)
            {
                Position = new Vector(correctedTableSize.x - Radius * 2, Position.y);
                velocity = new Vector(-velocity.x, velocity.y);
            }

            if (Position.y < 0)
            {
                Position = new Vector(Position.x, 0);
                velocity = new Vector(velocity.x, -velocity.y);
            }
            else if (Position.y + Radius * 2 > correctedTableSize.y)
            {
                Position = new Vector(Position.x, correctedTableSize.y - Radius * 2);
                velocity = new Vector(velocity.x, -velocity.y);
            }




            Velocity = velocity;
            RaiseNewPositionChangeNotification();
        }


        #endregion private
    }
}
