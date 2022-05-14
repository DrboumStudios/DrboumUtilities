namespace DrboumLibrary {
    public static class WaitSecondsForJobs {
        private const double _currentTimeDefault   = -2d;
        private const double _waitUntilTimeDefault = -1d;

        /// <summary>
        ///     This struct have the following functionalities
        ///     <list type="number">
        ///         <item> Start returning false if the boolean is not set in the constructor</item>
        ///         <item>return true if the time condition is satisfied</item>
        ///         <item>reset automatically when Tick is updated and the time condition is filled </item>
        ///     </list>
        /// </summary>
        public struct Interval : ITimer {
            private          double _waitUntilTime;
            private          double _currentTime;
            private readonly bool   _firstTickShouldReturnTrue;
            public           double WaitTime { get; private set; }

            public Interval(double waitTime, bool firstTickShouldReturnTrue = false)
            {
                WaitTime                   = waitTime;
                _firstTickShouldReturnTrue = firstTickShouldReturnTrue;
                if ( firstTickShouldReturnTrue ) {
                    _currentTime = 0;
                }
                else {
                    _currentTime = _currentTimeDefault;
                }

                _waitUntilTime = _waitUntilTimeDefault;
            }

            public bool Tick(double currentTime)
            {
                Start(currentTime);

                bool flag = _currentTime >= _waitUntilTime;
                if ( flag ) {
                    Reset();
                }

                return flag;
            }

            public void Start(double currentTime)
            {
                _currentTime = currentTime;
                if ( _waitUntilTime < 0f ) {
                    _waitUntilTime = _currentTime + WaitTime;
                }
            }

            public void Reset()
            {
                _currentTime   = _currentTimeDefault;
                _waitUntilTime = _waitUntilTimeDefault;
            }

            /// <summary>
            ///     Restart the instance to the creation state with the given parameters
            /// </summary>
            /// <param name="currentTime"></param>
            /// <param name="firstTickShouldReturnTrue"></param>
            public void Restart()
            {
                Reset();
                Start(_firstTickShouldReturnTrue ? 0 : -2d);
            }

            /// <summary>
            ///     Restart the instance to the creation state with the specified waitTime and the given parameters
            /// </summary>
            public void Restart(double waitTime)
            {
                WaitTime = waitTime;
                Restart();
            }
        }

        /// <summary>
        ///     This struct have the following functionalities
        ///     <list type="number">
        ///         <item>at first <see cref="Tick(double)" /> return true by default</item>
        ///         <item>
        ///             the first <see cref="Tick(double)" /> start the counting and will return false till the waittime is
        ///             reached
        ///         </item>
        ///         <item> Return true until the <see cref="Reset" /> function is called </item>
        ///     </list>
        /// </summary>
        public struct TimeOut : ITimer {
            private const double _firstTickReturnTrueValue = double.MinValue;
            private       double _waitUntilTime;
            private       double _currentTime;
            private       bool   _firstTickShouldReturnTrue;
            public        bool   Ready    => _currentTime >= _waitUntilTime;
            public        double TimeLeft => _waitUntilTime - _currentTime;

            public double WaitTime { get; private set; }

            public TimeOut(double waitTime, bool firstTickShouldReturnTrue = true)
            {
                WaitTime                   = waitTime;
                _firstTickShouldReturnTrue = firstTickShouldReturnTrue;
                _currentTime               = _currentTimeDefault;
                if ( _firstTickShouldReturnTrue ) {
                    _waitUntilTime = _firstTickReturnTrueValue;
                }
                else {
                    _waitUntilTime = _waitUntilTimeDefault;
                }
            }

            public bool Tick(double elapsedTime)
            {
                Start(elapsedTime);
                return elapsedTime >= _waitUntilTime;
            }

            public void Start(double currentTime)
            {
                _currentTime = currentTime;
                if ( _waitUntilTime < 0f && _waitUntilTime != _firstTickReturnTrueValue ) {
                    _waitUntilTime = _currentTime + WaitTime;
                }
            }

            public void Reset()
            {
                _currentTime   = _currentTimeDefault;
                _waitUntilTime = _waitUntilTimeDefault;
            }

            /// <summary>
            ///     Restart the instance to the creation state
            /// </summary>
            /// <param name="currentTime"></param>
            public void Restart()
            {
                Reset();
                if ( _firstTickShouldReturnTrue ) {
                    _waitUntilTime = _firstTickReturnTrueValue;
                }
            }

            /// <summary>
            ///     Restart the instance to the creation state with the specified waitTime with the same firstTickReturnTrue value as
            ///     set in the constructor or false otherwise
            /// </summary>
            public void Restart(double waitTime)
            {
                WaitTime = waitTime;
                Restart();
            }
            public void Restart(bool firstTickShouldReturnTrue, double waitTime = default)
            {
                _firstTickShouldReturnTrue = firstTickShouldReturnTrue;
                Restart(waitTime);
            }
        }
    }

    public interface ITimer {
        bool Tick(double elapsed);

        void Restart();
        void Restart(double waitTime);

        void Reset();
    }
}