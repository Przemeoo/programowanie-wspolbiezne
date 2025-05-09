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
        public Ball(Data.IBall ball) 
        {   dataBall = ball;
            dataBall.NewPositionNotification += RaisePositionChangeEvent;
        }
        #region IBall

        public event EventHandler<IPosition>? NewPositionNotification;

        public double Mass => dataBall.Mass;

        public double Radius => dataBall.Radius;



        #endregion IBall

        #region internal
        internal void WallCollision()
        {
            Data.Vector correctedTableSize = new Data.Vector(dataBall.TableSize.x - 8, dataBall.TableSize.y - 8);
            Data.Vector velocity = (Data.Vector)dataBall.Velocity;
            Data.Vector newPosition = new Data.Vector(dataBall.Position.x, dataBall.Position.y);

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


            dataBall.Position = newPosition;
            dataBall.Velocity = velocity;


        }

        internal void BallsCollision(Ball otherBall)
        {
            if (otherBall == this)
                return;

            Data.Vector newPosition = new Data.Vector(dataBall.Position.x, dataBall.Position.y);
            Data.IVector otherPosition = otherBall.dataBall.Position;
            double distance = Math.Sqrt(
                Math.Pow(newPosition.x + dataBall.Radius - (otherPosition.x + otherBall.dataBall.Radius), 2) +
                Math.Pow(newPosition.y + dataBall.Radius - (otherPosition.y + otherBall.dataBall.Radius), 2)
            );

            if (distance < (dataBall.Radius + otherBall.dataBall.Radius))
            {
                Data.Vector velocity = (Data.Vector)dataBall.Velocity;
                Data.Vector otherVelocity = (Data.Vector)otherBall.dataBall.Velocity;
                double m1 = dataBall.Mass;
                double m2 = otherBall.dataBall.Mass;

                Data.Vector deltaPos = new Data.Vector(
                    (newPosition.x + dataBall.Radius) - (otherPosition.x + otherBall.dataBall.Radius),
                    (newPosition.y + dataBall.Radius) - (otherPosition.y + otherBall.dataBall.Radius)
                );
                Data.Vector deltaVel = new Data.Vector(velocity.x - otherVelocity.x, velocity.y - otherVelocity.y);

                double dot = deltaVel.x * deltaPos.x + deltaVel.y * deltaPos.y;
                double mag = deltaPos.x * deltaPos.x + deltaPos.y * deltaPos.y;

                if (mag == 0 || dot >= 0)
                    return;

                double factor = 2 * dot / (mag * (m1 + m2));

                velocity = new Data.Vector(
                    velocity.x - factor * m2 * deltaPos.x,
                    velocity.y - factor * m2 * deltaPos.y
                );
                otherBall.dataBall.Velocity = new Data.Vector(
                    otherVelocity.x + factor * m1 * deltaPos.x,
                    otherVelocity.y + factor * m1 * deltaPos.y
                );

                double overlap = (dataBall.Radius + otherBall.dataBall.Radius) - distance;
                if (overlap > 0)
                {
                    double correction = overlap / 2; 
                    newPosition = new Data.Vector(
                        newPosition.x + (deltaPos.x / distance) * correction,
                        newPosition.y + (deltaPos.y / distance) * correction
                    );
                    Data.Vector otherNewPosition = new Data.Vector(
                        otherPosition.x - (deltaPos.x / distance) * correction,
                        otherPosition.y - (deltaPos.y / distance) * correction
                    );

                    dataBall.Position = newPosition;
                    otherBall.dataBall.Position = otherNewPosition;
                }

                dataBall.Velocity = velocity;

            }
        }

        #endregion internal

        #region private

        private readonly Data.IBall dataBall;

        

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
        }

        #endregion private
    }
}