

namespace TP.ConcurrentProgramming.Data
{
    internal class DiagnosticBuffer: IDisposable
    {
        private readonly DiagnosticLogEntry?[] buffer;
        private int readIndex;
        private int writeIndex;
        private int count;
        private readonly int maxSize;
        private readonly object bufferLock = new object();
        private readonly SemaphoreSlim dataAvailable;

        public DiagnosticBuffer(int size)
        {
            maxSize = size;
            buffer = new DiagnosticLogEntry?[size];
            readIndex = 0;
            writeIndex = 0;
            count = 0;
            dataAvailable = new SemaphoreSlim(0, size);
        }

        public bool TryAdd(DiagnosticLogEntry item)
        {
            lock (bufferLock)
            {
                if (count == maxSize)
                {
                    return false; 
                }

                buffer[writeIndex] = item;
                writeIndex = (writeIndex + 1) % maxSize;
                count++;
                dataAvailable.Release(); 
                return true;
            }
        }

        public bool TryTake(out DiagnosticLogEntry? item)
        {
            lock (bufferLock)
            {
                if (count == 0)
                {
                    item = null;
                    return false;
                }

                item = buffer[readIndex];
                buffer[readIndex] = null;
                readIndex = (readIndex + 1) % maxSize;
                count--;
                return true;
            }
        }

        public void WaitForData()
        {
            dataAvailable.Wait(); 
        }

        public void Dispose()
        {
            dataAvailable.Dispose();
        }

        public int Count
        {
            get
            {
                lock (bufferLock)
                {
                    return count;
                }
            }
        }
    }
}
