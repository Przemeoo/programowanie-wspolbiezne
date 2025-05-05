//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//  by introducing yourself and telling us what you do with this community.
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
      /// <summary>
      ///  Two dimensions immutable vector
      /// </summary>
      public record Vector : IVector
      {
            #region IVector

            public double x { get; init; }
            public double y { get; init; }

            #endregion IVector

            public Vector(double XComponent, double YComponent)
            {
                x = XComponent;
                y = YComponent;
            }
       }
}