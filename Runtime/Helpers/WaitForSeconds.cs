using System.Collections;
using UnityEngine;
namespace DrboumLibrary {
    public class WaitForSeconds : IEnumerator {
        private float _waitTime;
        private float _waitUntilTime;

        public WaitForSeconds(float time)
        {
            _waitTime      = time;
            _waitUntilTime = -1f;
        }

        public float WaitTime {
            get => _waitTime;
            set {
                _waitTime = value;
                Start();
            }
        }

        public bool   Ready   => Time.time >= _waitUntilTime && _waitUntilTime >= 0f;
        public object Current => null;

        public bool MoveNext()
        {
            Start();

            bool flag = Time.time < _waitUntilTime;
            if ( !flag ) {
                Reset();
            }

            return flag;
        }

        public void Reset()
        {
            _waitUntilTime = -1f;
        }

        public void Start()
        {
            if ( _waitUntilTime < 0f ) {
                _waitUntilTime = Time.time + WaitTime;
            }
        }
    }
}