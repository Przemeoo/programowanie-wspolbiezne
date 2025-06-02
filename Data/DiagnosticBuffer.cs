

namespace TP.ConcurrentProgramming.Data
{
    internal class DiagnosticBuffer
    {
        private readonly DiagnosticLogEntry?[] buffer;
        private int readIndex;
        private int writeIndex;
        private int count;
        private readonly int maxSize;
        private readonly object bufferLock = new object();

        public DiagnosticBuffer(int size)
        {
            maxSize = size;
            buffer = new DiagnosticLogEntry?[size];
            readIndex = 0;
            writeIndex = 0;
            count = 0;
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
    }
}
