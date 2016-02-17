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
        public bool IsEmpty { get { return val.Count() == 0; } }

        public Result()
        {

        }

        public Result(IEnumerable<Result> evalArgs)
        {
            var t = from x in evalArgs
                    group x by x.type into g
                    select new { type = g.Key, count = g.Count() };
            if (t.Count() == 1)
            {
                type = t.Single().type;
                val = evalArgs.Select(a => a.val).Aggregate((a, b) => a.Concat(b));
            }
            else
            {
                type = t.Max(a => a.type);
                val = evalArgs.Select(a => a.cast(type).val).Aggregate((a, b) => a.Concat(b));
            }
        }

        /// <summary>
        /// 类型
        /// </summary>
        public ResultType type { set; get; } = ResultType.Long;

        /// <summary>
        /// 变量值
        /// </summary>
        public IEnumerable<object> val { set; get; } = Enumerable.Empty<object>();

        public IEnumerable<long> castLong()
        {
            return castVal(ResultType.Long).Select(a => (long)a); ;
        }

        public IEnumerable<double> castDouble()
        {
            return castVal(ResultType.Double).Select(a => (double)a); ;
        }

        public Result cast(ResultType type)
        {
            if (this.type == type)
                return this;
            switch (type)
            {
                case ResultType.Long:
                    return new Result() { type = type, val = val.Select(a => Convert.ToInt64(a)).Cast<object>() };
                case ResultType.Double:
                    return new Result() { type = type, val = val.Select(a => Convert.ToDouble(a)).Cast<object>() };
            }
            return Result.Empty;
        }

        public IEnumerable<object> castVal(ResultType type)
        {
            if (this.type == type)
                return val;
            switch (type)
            {
                case ResultType.Long:
                    return val.Select(a => Convert.ToInt64(a)).Cast<object>();
                case ResultType.Double:
                    return val.Select(a => Convert.ToDouble(a)).Cast<object>();
            }
            return Enumerable.Empty<object>();
        }

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
                            v2.val.Select(a => lop(Convert.ToInt64(a), k1)).Cast<object>()
                        };
                    }
                    else
                    {
                        double k1 = Convert.ToInt64(k);
                        return new Result()
                        {
                            type = ResultType.Double,
                            val =
                            v2.val.Select(a => dop(Convert.ToDouble(a), k1)).Cast<object>()
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
                        v2.val.Select(a => dop(Convert.ToDouble(a), k1)).Cast<object>()
                    };
                }
            }
            if (v2.val.Count() <= 1)
            {
                return swap(v2, v1);
            }
            throw new SCException("不支持多个元素与多个元素之间的运算");
        }

        /// <summary>
        /// 向量化运算 - 强制转换
        /// </summary>
        /// <param name="conv"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Result par1(Func<object, object> conv, ResultType type)
        {
            return new Result() { type = type, val = val.Select(a => conv(a)) };
        }

        /// <summary>
        /// 向量化运算 - 动态转换
        /// </summary>
        /// <param name="conv1"></param>
        /// <param name="conv2"></param>
        /// <returns></returns>
        public Result par1a(Func<long, long> conv1, Func<double, double> conv2)
        {            
            switch (type)
            {
                case ResultType.Long:
                    return new Result() { type = type, val = castLong().Select(a => conv1(a)).Cast<object>() };
                case ResultType.Double:
                    return new Result() { type = type, val = castDouble().Select(a => conv2(a)).Cast<object>() };
                default:
                    return Result.Empty;
            }
        }

        /// <summary>
        /// 两个参数
        /// </summary>
        /// <param name="conv"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Result par2(Func<object, object, object> conv, ResultType type)
        {
            if (val.Count() != 2)
                throw new SCException("必须有两个参数");
            return new Result() { type = type, val = new List<object>() { conv(val.First(), val.Last()) } };
        }

        /// <summary>
        /// 归约
        /// </summary>
        /// <param name="conv1"></param>
        /// <param name="conv2"></param>
        /// <returns></returns>
        public Result par2a(Func<long, long, long> conv1, Func<double, double, double> conv2)
        {
            switch (type)
            {
                case ResultType.Long:
                    return new Result() { type = type, val = new List<object>() { castLong().Aggregate(conv1) } };
                case ResultType.Double:
                    return new Result() { type = type, val = new List<object>() { castDouble().Aggregate(conv2) } };
                default:
                    return Result.Empty;
            }
        }
    }

    static class ResultHelper
    {
        public static IEnumerable<long> castLong(this Result result)
        {
            return result.castVal(ResultType.Long).Select(a => (long)a); ;
        }

        public static IEnumerable<double> castDouble(this Result result)
        {
            return result.castVal(ResultType.Long).Select(a => (double)a); ;
        }
    }
}