using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Util
{
    class LazyRange : IEnumerable<object>
    {
        private long start;
        private long end;

        public LazyRange(long start, long end)
        {
            this.start = start;
            this.end = end;
        }

        public IEnumerator<object> GetEnumerator()
        {
            return new Enumerator(start, end);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(start, end);
        }

        class Enumerator : IEnumerator, IEnumerator<object>
        {
            private long start;
            private long end;
            private long ptr;
            private bool direction;

            public Enumerator(long start, long end)
            {
                this.start = start;
                this.end = end;
                Reset();
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.ptr;
                }
            }

            public object Current
            {
                get
                {
                    return this.ptr;
                }
            }

            public bool MoveNext()
            {
                if (this.direction)
                {
                    ptr++;
                    return ptr <= end;
                }
                else
                {
                    ptr--;
                    return ptr >= end;
                }
            }

            public void Reset()
            {
                this.direction = start <= end;
                this.ptr = start;
                if (this.direction)
                {
                    ptr--;
                }
                else
                {
                    ptr++;
                }
            }

            public void Dispose()
            {

            }
        }
    }

    static class LazyStringUtils
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
