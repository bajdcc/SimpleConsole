using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Typing
{
    public enum ResultType
    {
        Long,
        Double,
    }

    public class Result
    {
        public static readonly Result Empty = new Result();

        public Result()
        {

        }

        public Result(List<Result> evalArgs)
        {
            var l = new List<object>();
            foreach (var item in evalArgs)
            {
                l.AddRange(item.val);
            }
            type = l.Any(a => a is double) ? ResultType.Double : ResultType.Long;
            if (type == ResultType.Double)
            {
                val = l.ConvertAll(a => a is long ? (double)a : a);
            }
            val = l;
        }

        /// <summary>
        /// 类型
        /// </summary>
        public ResultType type { set; get; } = ResultType.Long;

        /// <summary>
        /// 变量值
        /// </summary>
        public IEnumerable<object> val { set; get; } = Enumerable.Empty<object>();

        public override string ToString()
        {
            return $"[{string.Join(", ", val ?? Enumerable.Empty<object>())}]";
        }

        public static Result operator +(Result v1, Result v2)
        {
            return oper(v1, v2, (a, b) => a + b, (a, b) => a + b, (a, b) => a + b);
        }

        public static Result operator -(Result v1, Result v2)
        {
            return oper(v1, v2, (a, b) => a - b, (a, b) => a - b, (a, b) => a - b);
        }

        public static Result operator *(Result v1, Result v2)
        {
            return oper(v1, v2, (a, b) => a * b, (a, b) => a * b, (a, b) => a * b);
        }

        public static Result operator /(Result v1, Result v2)
        {
            return oper(v1, v2, (a, b) => a / b, (a, b) => a / b, (a, b) => a / b);
        }

        public static Result operator %(Result v1, Result v2)
        {
            return oper(v1, v2, (a, b) => a % b, (a, b) => a % b, (a, b) => a % b);
        }

        public static Result oper(Result v1, Result v2, Func<Result, Result, Result> swap,
            Func<long, long, long> lop, Func<double, double, double> dop)
        {
            var c1 = v1.val.Count();
            if (c1 == 0)
            {
                return v2;
            }
            if (c1 == 1)
            {
                var k = v1.val.Single();
                if (v1.type == v2.type)
                {
                    if (v1.type == ResultType.Long)
                    {
                        long k1 = Convert.ToInt64(k);
                        return new Result()
                        {
                            type = ResultType.Long,
                            val =
                            new List<object>(v2.val).ConvertAll(a => lop(Convert.ToInt64(a), k1)).Cast<object>()
                        };
                    }
                    else
                    {
                        double k1 = Convert.ToInt64(k);
                        return new Result()
                        {
                            type = ResultType.Double,
                            val =
                            new List<object>(v2.val).ConvertAll(a => dop(Convert.ToDouble(a), k1)).Cast<object>()
                        };
                    }
                }
                else
                {
                    double k1 = Convert.ToDouble(k);
                    return new Result()
                    {
                        type = ResultType.Double,
                        val =
                        new List<object>(v2.val).ConvertAll(a => dop(Convert.ToDouble(a), k1)).Cast<object>()
                    };
                }
            }
            if (v2.val.Count() <= 1)
            {
                return swap(v2, v1);
            }
            throw new SCException("不支持多个元素与多个元素之间的运算");
        }

        public Result convert(Func<object, object> conv, ResultType type)
        {
            var l = new List<object>();
            foreach (var item in val)
            {
                l.Add(conv(item));
            }
            return new Result() { type = type, val = l };
        }
    }
}
