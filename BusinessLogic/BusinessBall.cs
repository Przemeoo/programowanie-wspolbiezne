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
        public Ball(Data.IBall ball, double tableWidth, double tableHeight)
        {
            dataBall = ball;
            TableSize = new Data.Vector(tableWidth, tableHeight);
            dataBall.NewPositionNotification += RaisePositionChangeEvent;
        }
        #region IBall

        public event EventHandler<IPosition>? NewPositionNotification;
        public double Mass => dataBall.Mass;
        public double Radius => dataBall.Radius;

        private readonly    Data.Vector TableSize;

        bool collision = false;
        #endregion IBall

        #region internal
        internal void WallCollision()
        {
            Data.Vector correctedTableSize = new Data.Vector(TableSize.x - 8, TableSize.y - 8);
            Data.Vector velocity = (Data.Vector)dataBall.Velocity;
            Data.Vector newPosition = new Data.Vector(dataBall.Position.x, dataBall.Position.y);

            if (newPosition.x <= 0 && velocity.x < 0)
            {
                velocity = new Data.Vector(-velocity.x, velocity.y);
                collision = true;
            }
            else if (newPosition.x + dataBall.Radius * 2 >= correctedTableSize.x && velocity.x > 0)
            {
                velocity = new Data.Vector(-velocity.x, velocity.y);
                collision = true;
            }

            if (newPosition.y <= 0 && velocity.y < 0)
            {
                velocity = new Data.Vector(velocity.x, -velocity.y);
                collision = true;
            }
            else if (newPosition.y + dataBall.Radius * 2 >= correctedTableSize.y && velocity.y > 0)
            {
                velocity = new Data.Vector(velocity.x, -velocity.y);
                collision = true;
            }

            if (collision)
            {
                dataBall.Velocity = velocity;
            }
            
            


        }

        internal void BallsCollision(Ball otherBall)
        {
            if (otherBall == this)
                return;

            double x1 = dataBall.Position.x + dataBall.Radius;
            double y1 = dataBall.Position.y + dataBall.Radius;
            double x2 = otherBall.dataBall.Position.x + otherBall.dataBall.Radius;
            double y2 = otherBall.dataBall.Position.y + otherBall.dataBall.Radius;
            double dx = x1 - x2;
            double dy = y1 - y2;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance >= dataBall.Radius + otherBall.dataBall.Radius || distance < 1e-10)
                return;

            Data.Vector v1 = (Data.Vector)dataBall.Velocity, v2 = (Data.Vector)otherBall.dataBall.Velocity;
            double dvx = v1.x - v2.x, dvy = v1.y - v2.y;
            double dot = dx * dvx + dy * dvy;
            if (dot >= 0)
                return;

            double m1 = dataBall.Mass, m2 = otherBall.dataBall.Mass;
            double factor = 2 * dot / (distance * distance * (m1 + m2));
            dataBall.Velocity = new Data.Vector(v1.x - factor * m2 * dx, v1.y - factor * m2 * dy);
            otherBall.dataBall.Velocity = new Data.Vector(v2.x + factor * m1 * dx, v2.y + factor * m1 * dy);

            double overlap = dataBall.Radius + otherBall.dataBall.Radius - distance;
            if (overlap > 0)
            {
                double correction = overlap / distance / (m1 + m2);
            }
        }

        #endregion internal

        #region private

        public readonly Data.IBall dataBall;



        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
        }

        #endregion private
    }
}