namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor

        internal Ball(Vector initialPosition, Vector initialVelocity, double radius)
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

        internal void Move(Vector delta)
        {
            Position = new Vector(Position.x + delta.x, Position.y + delta.y);
            RaiseNewPositionChangeNotification();
        }

        internal void UpdatePosition(double width, double height)
        {
            Vector velocity = (Vector)Velocity;
            Move(velocity);
            Vector pos = Position;

            // Kolizja z lewą/prawą krawędzią
            if (pos.x  - Radius < 0)
            {
                pos = new Vector(Radius, pos.y);
                velocity = new Vector(-velocity.x, velocity.y);
            }
            else if (pos.x + Radius > width)
            {
                pos = new Vector(width - Radius, pos.y);
                velocity = new Vector(-velocity.x, velocity.y);
            }

            // Kolizja z górą/dołem
            if (pos.y - Radius < 0)
            {
                pos = new Vector(pos.x, Radius);
                velocity = new Vector(velocity.x, -velocity.y);
            }
            else if (pos.y + Radius > height)
            {
                pos = new Vector(pos.x, height - Radius);
                velocity = new Vector(velocity.x, -velocity.y);
            }

            Position = pos;
            Velocity = velocity;
            RaiseNewPositionChangeNotification();
        }

        #endregion private
    }
}
