using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TP.ConcurrentProgramming.Data
{
    internal class DiagnosticLogger : IDiagnosticLogger, IDisposable
    {
        private static readonly Lazy<DiagnosticLogger> instance = new Lazy<DiagnosticLogger>(() => new DiagnosticLogger());
        private readonly ConcurrentQueue<DiagnosticLogEntry> logBuffer;
        private readonly Thread logThread;
        private volatile bool isRunning = true;
        private readonly StreamWriter logWriter;
        private readonly string logFilePath;
        private bool disposed = false;
        private const int MaxBufferSize = 1500;

        internal static DiagnosticLogger GetInstance()
        {
            return instance.Value;
        }

        private DiagnosticLogger()
        {
            logBuffer = new ConcurrentQueue<DiagnosticLogEntry>();
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

        public void Log(int eventType, int ballId1, double mass1, double positionX1, double positionY1, double velocityX1, double velocityY1,
                               int? ballId2 = null, double? mass2 = null, double? positionX2 = null, double? positionY2 = null, double? velocityX2 = null, double? velocityY2 = null)
        {
            if (isRunning && !disposed)
            {
                if (!Enum.IsDefined(typeof(LogEventType), eventType))
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid type: {eventType}");
                    return;
                }
                var logEntry = new DiagnosticLogEntry
                {
                    Timestamp = DateTime.Now,
                    MessageType = (LogEventType)eventType,
                    BallId1 = ballId1,
                    Mass1 = mass1,
                    PositionX1 = positionX1,
                    PositionY1 = positionY1,
                    VelocityX1 = velocityX1,
                    VelocityY1 = velocityY1,
                    BallId2 = ballId2,
                    Mass2 = mass2,
                    PositionX2 = positionX2,
                    PositionY2 = positionY2,
                    VelocityX2 = velocityX2,
                    VelocityY2 = velocityY2,
                };

                if (logBuffer.Count < MaxBufferSize)
                {
                    logBuffer.Enqueue(logEntry);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Buffer full, log entry discarded.");
                }
            }
        }

        private void LogToFile()
        {
            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                Converters = { new JsonStringEnumConverter() }
            };

            while (isRunning)
            {
                if (logBuffer.TryDequeue(out var logEntry))
                {
                    try
                    {
                        string json = JsonSerializer.Serialize(logEntry, options);
                        logWriter.WriteLine(json);
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
            try
            {
                isRunning = false;
                disposed = true;

                logThread?.Join(1000);

                var options = new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                while (logBuffer.TryDequeue(out var logEntry))
                {
                    try
                    {
                        string json = JsonSerializer.Serialize(logEntry, options);
                        logWriter.WriteLine(json);
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during resource disposal: {ex.Message}");
            }
        }


        internal class DiagnosticLogEntry
        {
            public DateTime Timestamp { get; set; }
            public LogEventType MessageType { get; set; }
            public int BallId1 { get; set; }
            public double Mass1 { get; set; }
            public double PositionX1 { get; set; }
            public double PositionY1 { get; set; }
            public double VelocityX1 { get; set; }
            public double VelocityY1 { get; set; }
            public int? BallId2 { get; set; }
            public double? Mass2 { get; set; }
            public double? PositionX2 { get; set; }
            public double? PositionY2 { get; set; }
            public double? VelocityX2 { get; set; }
            public double? VelocityY2 { get; set; }

        }


        public enum LogEventType
        {
            BallMovement,
            BallToBallCollision,
            WallCollisionTop,
            WallCollisionBottom,
            WallCollisionLeft,
            WallCollisionRight
        }

    }
}