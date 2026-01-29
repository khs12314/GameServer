using System;
using System.Collections.Generic;
using System.Text;

namespace NetWorkModule
{
    public interface IJobqueue
    {
        void Push(Action job);
    }
    public class JobQueue
    {
        Queue<Action> jobqueue = new Queue<Action>();
        object _lock = new object();
        bool _flush = false;
        public void Push(Action job)
        {
            bool flush = false;
            lock (_lock)
            {
                jobqueue.Enqueue(job);
                if (_flush == false)
                {
                    flush = _flush = true;
                }
            }
            if (flush)
            {
                Flush();
            }
        }
        void Flush()
        {
            while (true)
            {
                Action action = Pop();
                if (action == null) return;
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Job Error : {ex}");

                }
            }
        }
        Action Pop()
        {
            lock (_lock)
            {
                if (jobqueue.Count == 0)
                {
                    _flush = false;
                    return null;
                }
                return jobqueue.Dequeue();
            }
        }
    }
}

