//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
  public abstract class DataAbstractAPI : IDisposable
  {
        #region Layer Factory

        public static DataAbstractAPI GetDataLayer()
        {
            return modelInstance.Value;
        }
        public IDiagnosticLogger GetDiagnosticLogger()
        {
            return DiagnosticLogger.GetInstance();
        }

        #endregion Layer Factory

        #region public API

        public abstract void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler, double tableWidth, double tableHeight);

        #endregion public API

        #region IDisposable

        public abstract void Dispose();

        #endregion IDisposable

        #region private

        private static Lazy<DataAbstractAPI> modelInstance = new Lazy<DataAbstractAPI>(() => new DataImplementation());

        #endregion private
  }

    public interface IVector
    {
        double x { get; init; }
        double y { get; init; }
    }

    public interface IBall
    {
        event EventHandler<IVector> NewPositionNotification;

        IVector Velocity { get; }

        IVector Position { get; }

        void SetVelocity(double x, double y);

        double Mass { get; }

    }
    

    public interface IDiagnosticLogger : IDisposable
    {
        void Log(int eventType, int ballId1, double mass1, double positionX1, double positionY1, double velocityX1, double velocityY1,
                                int? ballId2 = null, double? mass2 = null, double? positionX2 = null, double? positionY2 = null, double? velocityX2 = null, double? velocityY2 = null);
    }
}

