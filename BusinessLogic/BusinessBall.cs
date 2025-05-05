//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Collections.Generic;
using Data = TP.ConcurrentProgramming.Data;
using BL = TP.ConcurrentProgramming.BusinessLogic;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        public Ball(Data.IBall ball, double tableWidth, double tableHeight, List<BL.Ball> allBalls) { dataBall = ball; TableSize = new Data.Vector(tableWidth, tableHeight); dataBall.NewPositionNotification += HandlePositionChange; AllBalls = allBalls; }
        #region IBall

        public event EventHandler<IPosition>? NewPositionNotification;

        public double Mass => dataBall.Mass;

        public void MoveTo(Data.IVector newPosition)
        {
            dataBall.MoveTo(newPosition);
        }

        #endregion IBall

        #region private

        private readonly Data.IBall dataBall;
        private readonly Data.IVector TableSize;
        private readonly List<BL.Ball> AllBalls;

        private void HandlePositionChange(object? sender, Data.IVector position)
        {
            Data.Vector correctedTableSize = new Data.Vector(TableSize.x - 4, TableSize.y - 4);
            Data.Vector velocity = (Data.Vector)dataBall.Velocity;
            Data.Vector newPosition = new Data.Vector(position.x, position.y);

            if (newPosition.x < 0)
            {
                newPosition = new Data.Vector(0, newPosition.y);
                velocity = new Data.Vector(-velocity.x, velocity.y);
            }
            else if (newPosition.x + dataBall.Radius * 2 > correctedTableSize.x)
            {
                newPosition = new Data.Vector(correctedTableSize.x - dataBall.Radius * 2, newPosition.y);
                velocity = new Data.Vector(-velocity.x, velocity.y);
            }

            if (newPosition.y < 0)
            {
                newPosition = new Data.Vector(newPosition.x, 0);
                velocity = new Data.Vector(velocity.x, -velocity.y);
            }
            else if (newPosition.y + dataBall.Radius * 2 > correctedTableSize.y)
            {
                newPosition = new Data.Vector(newPosition.x, correctedTableSize.y - dataBall.Radius * 2);
                velocity = new Data.Vector(velocity.x, -velocity.y);
            }

            lock (AllBalls) 
            {
                foreach (var other in AllBalls)
                {
                    if (other == this) continue;

                    Data.IVector otherPosition = other.dataBall.GetPosition();
                    double distance = Math.Sqrt(
                        Math.Pow(newPosition.x + dataBall.Radius - (otherPosition.x + other.dataBall.Radius), 2) +
                        Math.Pow(newPosition.y + dataBall.Radius - (otherPosition.y + other.dataBall.Radius), 2)
                    );

                    if (distance < (dataBall.Radius + other.dataBall.Radius))
                    {
                        // Sprężyste zderzenie
                        Data.Vector otherVelocity = (Data.Vector)other.dataBall.Velocity;
                        double m1 = dataBall.Mass;
                        double m2 = other.dataBall.Mass;

                        // Wektory różnicy pozycji i prędkości
                        Data.Vector deltaPos = new Data.Vector(
                            (newPosition.x + dataBall.Radius) - (otherPosition.x + other.dataBall.Radius),
                            (newPosition.y + dataBall.Radius) - (otherPosition.y + other.dataBall.Radius)
                        );
                        Data.Vector deltaVel = new Data.Vector(velocity.x - otherVelocity.x, velocity.y - otherVelocity.y);

                        // Iloczyn skalarny dla reakcji zderzenia
                        double dot = deltaVel.x * deltaPos.x + deltaVel.y * deltaPos.y;
                        double mag = deltaPos.x * deltaPos.x + deltaPos.y * deltaPos.y;

                        if (mag == 0 || dot >= 0) continue; // Unikanie dzielenia przez zero lub zderzeń nieistniejących

                        double factor = 2 * dot / (mag * (m1 + m2));

                        velocity = new Data.Vector(
                            velocity.x - factor * m2 * deltaPos.x,
                            velocity.y - factor * m2 * deltaPos.y
                        );
                        other.dataBall.Velocity = new Data.Vector(
                            otherVelocity.x + factor * m1 * deltaPos.x,
                            otherVelocity.y + factor * m1 * deltaPos.y
                        );

                        double overlap = (dataBall.Radius + other.dataBall.Radius) - distance;
                        double correction = overlap / (2 * distance);
                        newPosition = new Data.Vector(
                            newPosition.x + correction * deltaPos.x,
                            newPosition.y + correction * deltaPos.y
                        );
                        Data.Vector otherNewPosition = new Data.Vector(
                            otherPosition.x - correction * deltaPos.x,
                            otherPosition.y - correction * deltaPos.y
                        );
                        other.dataBall.MoveTo(otherNewPosition);
                    }
                }
            }

            dataBall.Velocity = velocity;
            NewPositionNotification?.Invoke(this, new Position(newPosition.x, newPosition.y));
        }

        #endregion private
    }
}