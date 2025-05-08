
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
            simulationTask?.Wait(); 
            cts?.Dispose();
            cts = null;
            simulationTask = null;
        }

        #region private

        private bool Disposed = false;
        private readonly UnderneathLayerAPI layerBellow;
        private readonly List<Ball> BallsList = new();
        private CancellationTokenSource cts;
        private Task simulationTask;

        private void StartSimulation()
        {
            cts = new CancellationTokenSource();
            simulationTask = Task.Run(() => RunSimulation(cts.Token), cts.Token);
        }

        private void RunSimulation(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                lock (BallsList) 
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

                Thread.Sleep(10); 
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