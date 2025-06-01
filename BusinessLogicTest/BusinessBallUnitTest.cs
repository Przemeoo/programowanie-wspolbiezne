//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{

    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void MoveTestMethod()
        {
            DataBallFixture dataBallFixture = new DataBallFixture();
            List<Ball> allBalls = new();
            IDiagnosticLogger logger = new TestLogger();
            Ball newInstance = new(dataBallFixture, 100, 100, 10, logger);
            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); Assert.IsNotNull(position); numberOfCallBackCalled++; };
            dataBallFixture.Move();
            Assert.AreEqual<int>(1, numberOfCallBackCalled);
        }

        #region testing instrumentation

        private class DataBallFixture : Data.IBall
        {
            public Data.IVector Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public event EventHandler<Data.IVector>? NewPositionNotification;
            public double Mass => 1.0;
            public double Radius => 1.0;

            public IVector Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IVector TableSize => throw new NotImplementedException();

            public void MoveTo(Data.IVector newPosition)
            {
            }
            public Data.IVector GetPosition()
            {
                return new VectorFixture(0.0, 0.0);
            }

            internal void Move()
            {
                NewPositionNotification?.Invoke(this, new VectorFixture(10.0, 10.0));
            }
        }

        private class VectorFixture : Data.IVector
        {
            internal VectorFixture(double X, double Y)
            {
                x = X; y = Y;
            }

            public double x { get; init; }
            public double y { get; init; }
        }
        private class TestLogger : IDiagnosticLogger
        {
            public void Log(string message)
            {
                
            }

            public void Stop()
            {
               
            }
        }


        #endregion testing instrumentation
    }
}