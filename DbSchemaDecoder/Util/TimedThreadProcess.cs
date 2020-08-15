using System;
using System.Threading;

namespace DbSchemaDecoder.Util
{

    interface IThreadableTask
    {
        event EventHandler OnThreadCompleted;
    }

    class TimedThreadProcess<T> where T : IThreadableTask
    {
        Thread _threadHandle;
        private readonly System.Timers.Timer _timer;
        DateTime _startTime;
        public event EventHandler<double> OnUpdate;
        T _instance;
        public event EventHandler<T> OnThreadCompletedEvent;

        public TimedThreadProcess(T instance, double updateInterval = 500)
        { 
            _timer = new System.Timers.Timer(updateInterval);
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = true;
            _instance = instance;
            _instance.OnThreadCompleted += OnThreadCompleted;
        }

        private void OnThreadCompleted(object sender, EventArgs e)
        {
            OnThreadCompletedEvent?.Invoke(this, _instance);
            Stop();
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var runTimeSec = (e.SignalTime - _startTime).TotalSeconds;
            OnUpdate?.Invoke(this, runTimeSec);
        }

        public double RunTimeInSec()
        {
            var runTimeSec = (DateTime.Now - _startTime).TotalSeconds;
            return runTimeSec;
        }

        public void Start(ThreadStart threadFunctor) 
        {
            _startTime = DateTime.Now;
            _timer.Start();
            _threadHandle = new Thread(threadFunctor);
            _threadHandle.Start();
        }

        public void Stop(bool abort = false)
        {
            if (_threadHandle != null)
            {
                if (abort)
                    _threadHandle.Abort();
                _threadHandle = null;
                _timer.Stop();
            }
        }
    }
}
