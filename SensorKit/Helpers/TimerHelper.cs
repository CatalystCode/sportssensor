using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
#endif

namespace SensorKit
{
#if !NETFX_CORE

    public delegate void TimerCallback(object state);

    public sealed class Timer : CancellationTokenSource, IDisposable
    {
        public Timer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            Task.Delay(dueTime, Token).ContinueWith(async (t, s) =>
            {
                var tuple = (Tuple<TimerCallback, object>)s;

                while (true)
                {
                    if (IsCancellationRequested)
                        break;
                    Task.Run(() => tuple.Item1(tuple.Item2));
                    await Task.Delay(period);
                }

            }, Tuple.Create(callback, state), CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.Default);
        }

        public new void Dispose() { base.Cancel(); }
    }


	/// <summary>
	/// TODO: Lack of Timer in PCL .NET assembles 
	/// </summary>
	public class TimerHelper : IDisposable
	{
        private Timer _timer;
        private TimeSpan _interval;

        /// <summary>
        /// Interval between signals in milliseconds.
        /// </summary>
        public double Interval
        {
            get { return _interval.TotalMilliseconds; }
            set { _interval = TimeSpan.FromMilliseconds(value); }
        }

        /// <summary>
        /// True if PCLTimer is running, false if not.
        /// </summary>
        public bool Enabled
        {
            get { return null != _timer; }
            set { if (value) Start(); else Stop(); }
        }

        /// <summary>
        /// Occurs when the specified time has elapsed and the PCLTimer is enabled.
        /// </summary>
        public event EventHandler Elapsed;

        /// <summary>
        /// Starts the PCLTimer.
        /// </summary>
        public void Start()
        {
            if (0 == _interval.TotalMilliseconds)
                throw new InvalidOperationException("Set Elapsed property before calling PCLTimer.Start().");
            _timer = new Timer(OnElapsed, null, _interval, _interval);
        }

        /// <summary>
        /// Stops the PCLTimer.
        /// </summary>
        public void Stop()
        {
            if(_timer != null)
                _timer.Dispose();
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            if (_timer != null)
                _timer.Dispose();
        }

        /// <summary>
        /// Invokes Elapsed event.
        /// </summary>
        /// <param name="state"></param>
        private void OnElapsed(object state)
        {
            if (null != _timer && null != Elapsed)
                Elapsed(this, EventArgs.Empty);
        }

	}

#else

    public class TimerHelper
    {
        //    DispatcherTimer _timer = new DispatcherTimer();

        //    /// <summary>
        //    /// Interbal in milliseconds
        //    /// </summary>
        //    public double Interval
        //    {
        //        get { return _timer.Interval.TotalMilliseconds; }
        //        set { _timer.Interval = TimeSpan.FromMilliseconds(value); }
        //    }

        //    /// <summary>
        //    /// Occurs when the specified time has elapsed and the PCLTimer is enabled.
        //    /// </summary>
        //    public event EventHandler Elapsed;

        //    public TimerHelper() : base()
        //    {
        //        _timer.Tick += TimerHelper_Tick;
        //    }

        //    private void TimerHelper_Tick(object sender, object e)
        //    {
        //        if (Elapsed != null)
        //        {
        //            Elapsed(this, EventArgs.Empty);
        //        }
        //    }

        //    public void Dispose()
        //    {
        //        if (_timer != null)
        //        {
        //            _timer.Stop();
        //            _timer = null;
        //        }
        //    }

        //    public void Start()
        //    {
        //        _timer.Start();
        //    }

        //    public void Stop()
        //    {
        //        _timer.Stop();
        //    }

        //}

        //public class TimerHelper : IDisposable
        //{
            Timer _timer;
            TimeSpan _interval = TimeSpan.FromMilliseconds(1000);

            public double Interval
            {
                get
                {
                    return _interval.TotalMilliseconds;
                }
                set
                {
                    _interval = TimeSpan.FromMilliseconds(value);
                }
            }

            /// <summary>
            /// Occurs when the specified time has elapsed and the PCLTimer is enabled.
            /// </summary>
            public event EventHandler Elapsed;

            public TimerHelper()
            {
            }

            public void Dispose()
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                }
            }

            public void Start()
            {
                Debug.WriteLine($"TIMER START...{(int)_interval.TotalMilliseconds}");
                _timer = new Timer(TimerCallback, null, 0, (int)_interval.TotalMilliseconds);
            }

            public void Stop()
            {
                Debug.WriteLine("TIMER STOP...");
                Dispose();
            }

            private async void TimerCallback(object state)
            {
                //Logger.WriteLine("TIMER TICK...");
                if (Elapsed != null)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Elapsed(this, EventArgs.Empty);
                    });
                }
            }


        }

#endif

    }

