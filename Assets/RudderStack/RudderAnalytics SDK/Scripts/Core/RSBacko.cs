using System;
using System.Threading.Tasks;

namespace RudderStack.Request
{
    public class RSBacko
    {
        private readonly int  _max;
        private          int  _attempt;

        private int _currentAttemptTime;

        private const int IntMax = int.MaxValue - ushort.MaxValue;


        public bool HasReachedMax  => _currentAttemptTime >= _max;
        public int  CurrentAttempt => _attempt;

        public RSBacko(int max = 10000)
        {
            _max    = Math.Min(max, IntMax);
        }

#if !NET35
        public Task AttemptAsync()
        {
            return Task.Delay(AttemptTime());
        }

        public Task AttemptForAsync(int attempt)
        {
            return Task.Delay(AttemptTimeFor(attempt));
        }
#endif

        public int AttemptTime()
        {
            return _currentAttemptTime = AttemptTimeFor(_attempt++);
        }

        public int AttemptTimeFor(int attempt)
        {
            return Math.Min(_max, attempt + 1);
        }

        public void Reset()
        {

            _attempt = _currentAttemptTime = 0;
        }
    }
}