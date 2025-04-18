﻿//____________________________________________________________________________________________________________________________________
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
            MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(20));
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

                Vector startingPosition = new(
                  radius + random.NextDouble() * (tableWidth - radius * 2),
                  radius + random.NextDouble() * (tableHeight - radius * 2)
                );


                Vector initialVelocity = new(
                    (random.NextDouble() - 0.5) * 5,
                    (random.NextDouble() - 0.5) * 5
                );

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
                    MoveTimer.Dispose();
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

        private readonly Timer MoveTimer;
        private List<Ball> BallsList = new();

        private readonly object _lock = new();
        private void Move(object? state)
        {
            if (TableSize == null)
                return;
            lock (_lock)
            {
                var ballCopy = BallsList.ToList();
                foreach (Ball ball in ballCopy)
                {
                    ball.Move(TableSize);
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
