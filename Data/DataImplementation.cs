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
                BallsList.Add(newBall);

                _moveTasks.Add(Task.Factory.StartNew(
                    () => MoveBallAsync(newBall, _cancellationTokenSource.Token).GetAwaiter().GetResult(),
                    _cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default));
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
                    try
                    {
                        Task.WhenAll(_moveTasks).Wait(); 
                    }
                    catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException))
                    {

                    }
                    _cancellationTokenSource?.Dispose();
                    BallsList.Clear();
                    _moveTasks.Clear();
                }
                Disposed = true;
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private bool Disposed = false;
        private readonly List<Ball> BallsList = new();
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly List<Task> _moveTasks = new();
        private readonly object _lock = new();

        private async Task MoveBallAsync(Ball ball, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    lock (_lock) 
                    {
                        ball.Move();
                    }
                    await Task.Delay(30, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
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