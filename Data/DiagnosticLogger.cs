using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace TP.ConcurrentProgramming.Data
{
    internal class DiagnosticLogger : IDiagnosticLogger, IDisposable
    {
        private static readonly Lazy<DiagnosticLogger> _instance = new Lazy<DiagnosticLogger>(() => new DiagnosticLogger());
        public static DiagnosticLogger Instance => _instance.Value;

        private readonly DiagnosticBuffer logBuffer;
        private readonly Thread logThread;
        private volatile bool isRunning = true;
        private readonly StreamWriter logWriter;
        private readonly string logFilePath;
        private readonly object fileLock = new object();
        private bool disposed = false;
        private const int MaxBufferSize = 1500;

        private DiagnosticLogger()
        {
            logBuffer = new DiagnosticBuffer(MaxBufferSize);
            string projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
            string logsDirectory = Path.Combine(projectDirectory, "Logs");
            Directory.CreateDirectory(logsDirectory);

            foreach (var file in Directory.GetFiles(logsDirectory, "diagnosticsLogs_*.json"))
            {
                if (File.GetCreationTime(file) < DateTime.Now.AddDays(-7))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException ex) 
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to delete old log file '{file}': {ex.Message}");
                    }
                }
            }

            string dateName = DateTime.Now.ToString("dd.MM.yyyy_HH.mm");
            logFilePath = Path.Combine(logsDirectory, $"diagnosticsLogs_{dateName}.json");

            logWriter = new StreamWriter(logFilePath, append: false, Encoding.UTF8) { AutoFlush = true };
            logThread = new Thread(LogToFile);
            logThread.Start();
        }


        public void Log(DiagnosticLogEntry logEntry)
        {
            if (isRunning && !disposed)
            {
                logEntry.Timestamp = DateTime.Now; // Ustawiamy czas w loggerze
                if (!logBuffer.TryAdd(logEntry))
                {
                    System.Diagnostics.Debug.WriteLine("Buffer full, log entry discarded.");
                }
            }
        }
        public void LogCollision(int ballId1, double mass1, double positionX1, double positionY1, double velocityX1, double velocityY1,
                                CollisionType collisionType, string message)
        {
            if (isRunning && !disposed)
            {
                var logEntry = new DiagnosticLogEntry
                {
                    Timestamp = DateTime.Now,
                    BallId1 = ballId1,
                    Mass1 = mass1,
                    PositionX1 = positionX1,
                    PositionY1 = positionY1,
                    VelocityX1 = velocityX1,
                    VelocityY1 = velocityY1,
                    CollisionType = collisionType,
                    Message = message
                };

                if (!logBuffer.TryAdd(logEntry))
                {
                    System.Diagnostics.Debug.WriteLine("Buffer full, log entry discarded.");
                }
            }
        }
        private void LogToFile()
        {
            while (isRunning)
            {
                if (logBuffer.TryTake(out var logEntry))
                {
                        try
                        {
                            lock (fileLock)
                            {
                                string json = JsonSerializer.Serialize(logEntry);
                                logWriter.WriteLine(json);
                            }
                        }
                        catch (IOException ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error writing to log file: {ex.Message}");
                        }
                        catch (JsonException ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error serializing log entry: {ex.Message}");
                        }
                    }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            isRunning = false;
            disposed = true;

            logThread.Join();

            while (logBuffer.TryTake(out var logEntry))
            {
                try
                {
                    lock (fileLock)
                    {
                        string json = JsonSerializer.Serialize(logEntry);
                        logWriter.WriteLine(json);
                    }
                }
                catch (IOException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error writing to log file during dispose: {ex.Message}");
                }
                catch (JsonException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error serializing log entry during dispose: {ex.Message}");
                }
            }

            logWriter?.Dispose();
        }
    }

    internal class DiagnosticLogEntry
    {
        public DateTime Timestamp { get; set; }
        public int BallId1 { get; set; }
        public double Mass1 { get; set; }
        public double PositionX1 { get; set; }
        public double PositionY1 { get; set; }
        public double VelocityX1 { get; set; }
        public double VelocityY1 { get; set; }
        public CollisionType? CollisionType { get; set; }
        public string? Message { get; set; }
    }

    public enum CollisionType
    {
        WallLeft,
        WallRight,
        WallTop,
        WallBottom,
        BallToBall
    }

}