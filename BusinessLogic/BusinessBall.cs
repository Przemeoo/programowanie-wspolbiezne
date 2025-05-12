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
        private readonly Data.IBall dataBall;
        private readonly List<Ball> otherBalls;
        private readonly object collisionLock;
        private readonly CancellationTokenSource cts;
        private readonly Task collisionTask;
        private bool disposed = false;

        public Ball(Data.IBall ball, List<Ball> otherBalls, object collisionLock)
        {
            dataBall = ball;
            this.otherBalls = otherBalls;
            this.collisionLock = collisionLock;
            dataBall.NewPositionNotification += RaisePositionChangeEvent;
            cts = new CancellationTokenSource();
            collisionTask = Task.Run(() => DetectCollisionsAsync(cts.Token), cts.Token);
        }

        public event EventHandler<IPosition>? NewPositionNotification;
        public double Mass => dataBall.Mass;
        public double Radius => dataBall.Radius;
        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            cts.Cancel();
            try
            {
                collisionTask.Wait();
            }
            catch (AggregateException) { }
            cts.Dispose();
        }
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
                dataBall.Position = new Data.Vector(dataBall.Position.x + dx * correction * m2, dataBall.Position.y + dy * correction * m2);
                otherBall.dataBall.Position = new Data.Vector(otherBall.dataBall.Position.x - dx * correction * m1, otherBall.dataBall.Position.y - dy * correction * m1);
            }
        }

        private async Task DetectCollisionsAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    lock (collisionLock)
                    {
                        WallCollision();
                        foreach (var otherBall in otherBalls)
                        {
                            if (otherBall != this)
                                BallsCollision(otherBall);
                        }
                    }
                    await Task.Delay(20, token);
                }
            }
            catch (OperationCanceledException) 
            { 
            }
            catch (Exception) 
            { 
            }
        }

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
        }


    }
}