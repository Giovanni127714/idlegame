using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace Idlegame.Services
{
    /// <summary>
    /// Drives the game update loop on the UI thread via a DispatcherTimer,
    /// reporting real elapsed time per tick so income accrual stays accurate
    /// even if a tick fires late.
    /// </summary>
    public class GameLoopService
    {
        private readonly DispatcherTimer _timer;
        private readonly Stopwatch _stopwatch = new();

        public event Action<double>? Tick;

        public GameLoopService(TimeSpan? interval = null)
        {
            _timer = new DispatcherTimer
            {
                Interval = interval ?? TimeSpan.FromMilliseconds(50) // 20x/sec, ruim boven de vereiste 10x/sec
            };
            _timer.Tick += OnTimerTick;
        }

        public void Start()
        {
            _stopwatch.Restart();
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            _stopwatch.Stop();
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            double elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
            _stopwatch.Restart();
            Tick?.Invoke(elapsedSeconds);
        }
    }
}
