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

            StartCollisionDetection();
        }

        public void Stop()
        {
            cts?.Cancel();
            try
            {
                if (collisionTasks != null)
                    Task.WhenAll(collisionTasks).Wait();
            }
            catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException))
            {
            }
            cts?.Dispose();
            cts = null;
            collisionTasks = null;
        }

        #region private

        private bool Disposed = false;
        private readonly UnderneathLayerAPI layerBellow;
        private readonly List<Ball> BallsList = new();
        private CancellationTokenSource cts;
        private List<Task> collisionTasks;
        private readonly object collisionLock = new();

        private void StartCollisionDetection()
        {
            cts = new CancellationTokenSource();
            collisionTasks = new List<Task>();

            collisionTasks.Add(Task.Factory.StartNew(
                () => RunCollisionDetection(cts.Token).GetAwaiter().GetResult(),
                cts.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default));
        }

        private async Task RunCollisionDetection(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                lock (collisionLock)
                {
                    foreach (var ball in BallsList)
                    {
                        ball.WallCollision();
                    }

                    for (int i = 0; i < BallsList.Count; i++)
                    {
                        for (int j = i + 1; j < BallsList.Count; j++)
                        {
                            BallsList[i].BallsCollision(BallsList[j]);
                        }
                    }
                }
                await Task.Delay(10);
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