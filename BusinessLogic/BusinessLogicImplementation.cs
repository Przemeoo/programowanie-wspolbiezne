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
            layerBellow.Dispose();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler, double tableWidth, double tableHeight)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            Stop();

            BallsList.Clear();
            layerBellow.Start(numberOfBalls, (startingPosition, databall) =>
            {
                var ball = new Ball(databall);
                BallsList.Add(ball);
                upperLayerHandler(new Position(startingPosition.x, startingPosition.y), ball);
            }, tableWidth, tableHeight);

            StartSimulation();
        }

        public void Stop()
        {
            cts?.Cancel();
            try
            {
                if (ballTasks != null)
                    Task.WhenAll(ballTasks).Wait();
            }
            catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException))
            {

            }
            cts?.Dispose();
            cts = null;
            ballTasks = null;
        }

        #region private

        private bool Disposed = false;
        private readonly UnderneathLayerAPI layerBellow;
        private readonly List<Ball> BallsList = new();
        private CancellationTokenSource cts;
        private List<Task> ballTasks;
        private readonly object collisionLock = new();

        private void StartSimulation()
        {
            cts = new CancellationTokenSource();
            ballTasks = new List<Task>();

            foreach (var ball in BallsList)
            {
                ballTasks.Add(Task.Factory.StartNew(
                    () => RunBallSimulation(ball, cts.Token).GetAwaiter().GetResult(),
                    cts.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default));
            }
        }

        private async Task RunBallSimulation(Ball ball, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                lock (collisionLock)
                {
                    ball.WallCollision();
                }

                lock (collisionLock)
                {
                    foreach (var otherBall in BallsList)
                    {
                        if (otherBall != ball)
                        {
                            ball.BallsCollision(otherBall);
                        }
                    }
                }

                 ((Data.Ball)ball.dataBall).Move();

                await Task.Delay(10, cancellationToken);
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