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
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        #region ctor

        public BusinessLogicImplementation() : this(null)
        { }

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
        }

        #endregion ctor

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            Stop();
            lock (collisionLock)
            {
                BallsList.Clear();
            }
            layerBellow.Dispose();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler, double tableWidth, double tableHeight)
        {
            double radius = 0.03 * tableHeight;
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            Stop();
            
            BallsList.Clear();
            layerBellow.Start(numberOfBalls, (startingPosition, databall) =>
            {
                lock (collisionLock)
                {
                    var ball = new Ball(databall, tableWidth, tableHeight, radius);
                    upperLayerHandler(new Position(startingPosition.x, startingPosition.y), ball);
                    BallsList.Add(ball);
                }
            }, tableWidth, tableHeight);

            StartCollisionDetection();
        }

        public void Stop()
        {
            if (cts == null)
                return;

            cts.Cancel();
            try
            {
                if (collisionTasks != null)
                {
                    Task.WhenAll(collisionTasks).Wait();
                }
            }
            catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException))
            {
            }

            cts.Dispose();
            cts = null;
            collisionTasks = null;
        }

        #region private

        private bool Disposed = false;
        private readonly UnderneathLayerAPI layerBellow;
        private readonly List<Ball> BallsList = new();
        private CancellationTokenSource? cts;
        private List<Task>? collisionTasks;
        private readonly object collisionLock = new();

        private void StartCollisionDetection()
        {
            cts = new CancellationTokenSource();
            collisionTasks = new List<Task>();

            foreach (var ball in BallsList)
            {
                collisionTasks.Add(Task.Run(
                    () => CheckCollisionsForBallAsync(ball, cts.Token),
                    cts.Token));
            }
        }

        private async Task CheckCollisionsForBallAsync(Ball ball, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    ball.WallCollision();
                    lock (collisionLock)
                    {
                        foreach (var otherBall in BallsList)
                        {
                            if (otherBall != ball)
                                ball.BallsCollision(otherBall);
                        }
                    }
                    await Task.Delay(20, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }

        }

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}