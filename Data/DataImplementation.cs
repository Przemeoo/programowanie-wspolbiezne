//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor

        public DataImplementation()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        #endregion ctor


        private Vector? TableSize;

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler, double tableWidth, double tableHeight)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            TableSize = new Vector(tableWidth, tableHeight);

            Random random = new Random();
            double radius = 0.03 * tableHeight;

            for (int i = 0; i < numberOfBalls; i++)
            {
                Vector startingPosition;

                bool validPosition;
                int maxAttempts = 100;
                do
                {
                    startingPosition = new Vector(
                        radius + random.NextDouble() * (tableWidth - radius * 2),
                        radius + random.NextDouble() * (tableHeight - radius * 2)
                    );
                    validPosition = true;

                    foreach (var existingBall in BallsList)
                    {
                        double distance = Math.Sqrt(
                            Math.Pow(startingPosition.x + radius - (existingBall.Position.x + existingBall.Radius), 2) +
                            Math.Pow(startingPosition.y + radius - (existingBall.Position.y + existingBall.Radius), 2)
                        );
                        if (distance < (radius + existingBall.Radius))
                        {
                            validPosition = false;
                            break;
                        }
                    }
                    maxAttempts--;
                } while (!validPosition && maxAttempts > 0);

                if (!validPosition)
                {
                    throw new InvalidOperationException("Nie można znaleźć wolnej pozycji dla kulki po maksymalnej liczbie prób.");
                }

                Vector initialVelocity = new Vector((random.NextDouble() - 0.5) * 5, (random.NextDouble() - 0.5) * 5);

                Ball newBall = new(startingPosition, initialVelocity, tableWidth, tableHeight, radius);
                upperLayerHandler(startingPosition, newBall);

                lock (_lock)
                {
                    BallsList.Add(newBall);
                }
            }
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    lock (_lock)
                    {
                        foreach (var ball in BallsList)
                        {
                            ball.Dispose();
                        }
                        BallsList.Clear();
                    }
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private bool Disposed = false;
        private readonly List<Ball> BallsList = new();
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly List<Task> _moveTasks = new();
        private readonly object _lock = new();

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            returnBallsList(BallsList);
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            returnNumberOfBalls(BallsList.Count);
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}