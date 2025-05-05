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
            StartMovingAsync(_cancellationTokenSource.Token);
        }

        #endregion ctor

        #region DataAbstractAPI

        private Vector? TableSize;

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler, double tableWidth, double tableHeight)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            TableSize = new Vector(tableWidth, tableHeight);

            Random random = new Random();

            for (int i = 0; i < numberOfBalls; i++)
            {
                double radius = 12;
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
                            Math.Pow(startingPosition.x + radius - (existingBall.GetPosition().x + existingBall.Radius), 2) +
                            Math.Pow(startingPosition.y + radius - (existingBall.GetPosition().y + existingBall.Radius), 2)
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

                double minSpeed = 0.2;
                double vx, vy;
                do
                {
                    vx = (random.NextDouble() - 0.5) * 2;
                } while (Math.Abs(vx) < minSpeed);
                do
                {
                    vy = (random.NextDouble() - 0.5) * 2;
                } while (Math.Abs(vy) < minSpeed);

                Vector initialVelocity = new Vector(vx, vy);

                Ball newBall = new(startingPosition, initialVelocity, tableWidth, tableHeight, radius);
                upperLayerHandler(startingPosition, newBall);
                BallsList.Add(newBall);
            }
        }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource?.Dispose();
                    BallsList.Clear();
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
        private readonly object _lock = new();

        private async void StartMovingAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await MoveAsync();
                    await Task.Delay(20, cancellationToken); 
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task MoveAsync()
        {
            if (TableSize == null)
                return;

            lock (_lock)
            {
                var ballCopy = BallsList.ToList();
                foreach (Ball ball in ballCopy)
                {
                    ball.Move();
                }
            }
        }

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
