using System;

namespace Assets.Scripts.Utils
{
    public class TimeIntervalLimiter
    {
        public DateTime LastTime { get; private set; }

        public TimeSpan Interval { get; set; }

        public void Do(Action action, Action ifSkip = null)
        {
            if (Elapsed < Interval)
            {
                ifSkip?.Invoke();
                return;
            }

            try
            {
                action();
                LastTime = DateTime.Now;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public TimeSpan Elapsed => DateTime.Now - LastTime;

        public TimeIntervalLimiter(TimeSpan interval)
        {
            LastTime = DateTime.MinValue;
            Interval = interval;
        }
    }
}
