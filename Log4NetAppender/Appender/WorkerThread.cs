using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Log4NetAppender.Appender
{
    public class WorkerThread<T> : IDisposable
    {
        public WorkerThread(string name, TimeSpan interval, Action<T[]> processor)
        {
            _interval = interval;
            _processor = processor;
            _queue = new ConcurrentQueue<T>();
            _disposeEvent = new AutoResetEvent(false);
            _thread = new Thread(Loop) { Name = name, IsBackground = true };
            _thread.Start();
        }

        private readonly ConcurrentQueue<T> _queue;
        private readonly AutoResetEvent _disposeEvent;
        private readonly Thread _thread;
        private readonly TimeSpan _interval;
        private readonly Action<T[]> _processor;

        public void Enqueue(T item)
        {
            _queue.Enqueue(item);
        }

        public void Dispose()
        {
            _disposeEvent.Set();
            _thread.Join();
        }

        private void Loop()
        {
            while (true)
            {
                if (_disposeEvent.WaitOne(_interval))
                {
                    Dequeue();
                    return;
                }
                Dequeue();
            }
        }

        private void Dequeue()
        {
            var count = _queue.Count;
            if (count <= 0)
            {
                return;
            }
            var items = new T[count];
            for (var i = 0; i < count; i++)
            {
                _queue.TryDequeue(out items[i]);
            }
            _processor.Invoke(items);
        }
    }
}