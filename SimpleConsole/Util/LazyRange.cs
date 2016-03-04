using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SimpleConsole.Util
{
    internal class LazyRange : IEnumerable<object>
    {
        private readonly long _start;
        private readonly long _end;

        public LazyRange(long start, long end)
        {
            _start = start;
            _end = end;
        }

        public IEnumerator<object> GetEnumerator()
        {
            return new Enumerator(_start, _end);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(_start, _end);
        }

        private class Enumerator : IEnumerator<object>
        {
            private readonly long _start;
            private readonly long _end;
            private long _ptr;
            private bool _direction;

            public Enumerator(long start, long end)
            {
                _start = start;
                _end = end;
                Reset();
            }

            object IEnumerator.Current => _ptr;

            public object Current => _ptr;

            public bool MoveNext()
            {
                if (_direction)
                {
                    _ptr++;
                    return _ptr <= _end;
                }
                else
                {
                    _ptr--;
                    return _ptr >= _end;
                }
            }

            public void Reset()
            {
                _direction = _start <= _end;
                _ptr = _start;
                if (_direction)
                {
                    _ptr--;
                }
                else
                {
                    _ptr++;
                }
            }

            public void Dispose()
            {

            }
        }
    }

    internal static class LazyStringUtils
    {
        public static void Join(this string separator, IEnumerable<object> values, TextWriter output)
        {
            if (values == null || separator == null)
            {
                return;
            }
            using (var enumerator = values.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    if (enumerator.Current != null)
                    {
                        output.Write(enumerator.Current);
                    }
                    while (enumerator.MoveNext())
                    {
                        output.Write(separator);
                        if (enumerator.Current != null)
                        {
                            output.Write(enumerator.Current);
                        }
                    }
                }
            }
        }
    }
}
