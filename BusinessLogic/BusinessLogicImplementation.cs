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
using TP.ConcurrentProgramming.Data;
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
            logger = UnderneathLayerAPI.GetLogger();
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
                    var ball = new Ball(databall, tableWidth, tableHeight, radius, logger);
                    ball.NewPositionNotification += (sender, position) => CheckCollisionsForBall(ball); 
                    upperLayerHandler(new Position(startingPosition.x, startingPosition.y), ball);
                    BallsList.Add(ball);
                }
            }, tableWidth, tableHeight);
        }

        public void Stop()
        {
            lock (collisionLock)
            {
                foreach (var ball in BallsList)
                {
                    ball.NewPositionNotification -= (sender, position) => CheckCollisionsForBall(ball); 
                }
            }
        }

        #region private

        private bool Disposed = false;
        private readonly UnderneathLayerAPI layerBellow;
        private readonly IDiagnosticLogger logger;
        private readonly List<Ball> BallsList = new();
        private readonly object collisionLock = new();

        private void CheckCollisionsForBall(Ball ball)
        {
            lock (collisionLock)
            {
                ball.WallCollision();
                foreach (var otherBall in BallsList)
                {
                    if (otherBall != ball)
                        ball.BallsCollision(otherBall);
                }
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