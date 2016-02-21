using SimpleConsole.Expression;
using SimpleConsole.Util;
using System;
using System.Collections.Generic;
using System.IO;
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

    public enum CompareType
    {
        Equal,
        NotEqual,
        LessThan,
        NotLessThan,
        GreaterThan,
        NotGreaterThan,
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
            if (t.Count() == 0)
                return;
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

        public bool Bool()
        {
            return Convert.ToBoolean(val.First());
        }

        public IEnumerable<long> castLong()
        {
            return castVal(ResultType.Long).Select(a => (long)a); ;
        }

        public IEnumerable<double> castDouble()
        {
            return castVal(ResultType.Double).Select(a => (double)a);
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

        public Result compare(CompareType cmp)
        {
            bool r;
            switch (type)
            {
                case ResultType.Long:
                    {
                        var l = castLong();
                        var x = l.First();
                        var y = l.Last();
                        switch (cmp)
                        {
                            case CompareType.Equal:
                                r = x == y;
                                break;
                            case CompareType.NotEqual:
                                r = x != y;
                                break;
                            case CompareType.LessThan:
                                r = x < y;
                                break;
                            case CompareType.NotLessThan:
                                r = x >= y;
                                break;
                            case CompareType.GreaterThan:
                                r = x > y;
                                break;
                            case CompareType.NotGreaterThan:
                                r = x <= y;
                                break;
                            default:
                                throw new SCException("未知的比较运算符");
                        }
                        return new Result() { val = new List<object> { r } };
                    }
                case ResultType.Double:
                    {
                        var l = castDouble();
                        var x = l.First();
                        var y = l.Last();
                        switch (cmp)
                        {
                            case CompareType.Equal:
                                r = x == y;
                                break;
                            case CompareType.NotEqual:
                                r = x != y;
                                break;
                            case CompareType.LessThan:
                                r = x < y;
                                break;
                            case CompareType.NotLessThan:
                                r = x >= y;
                                break;
                            case CompareType.GreaterThan:
                                r = x > y;
                                break;
                            case CompareType.NotGreaterThan:
                                r = x <= y;
                                break;
                            default:
                                throw new SCException("未知的比较运算符");
                        }
                        return new Result() { val = new List<object> { r } };
                    }
                default:
                    throw new SCException("未知的比较数据类型");
            }
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", val ?? Enumerable.Empty<object>())}]";
        }

        public virtual void ToString(TextWriter output)
        {
            output.Write("[");
            ", ".Join(val, output);
            output.Write("[");
        }

        public string GetTypeString()
        {
            return $"{string.Concat(val.GetType().Name.TakeWhile(char.IsLetter))} :: {type}";
        }

        public static Result operator +(Result v1, Result v2)
        {
            return oper(v1, v2, (a, b) => a + b, (a, b) => a + b, (b, a) => a + b, (b, a) => a + b);
        }

        public static Result operator -(Result v1, Result v2)
        {
            return oper(v1, v2, (a, b) => a - b, (a, b) => a - b, (b, a) => a - b, (b, a) => a - b);
        }

        public static Result operator *(Result v1, Result v2)
        {
            return oper(v1, v2, (a, b) => a * b, (a, b) => a * b, (b, a) => a * b, (b, a) => a * b);
        }

        public static Result operator /(Result v1, Result v2)
        {
            return oper(v1, v2, (a, b) => a / b, (a, b) => a / b, (b, a) => a / b, (b, a) => a / b);
        }

        public static Result operator %(Result v1, Result v2)
        {
            return oper(v1, v2, (a, b) => a % b, (a, b) => a % b, (b, a) => a % b, (b, a) => a % b);
        }

        private static Result oper(Result v1, Result v2,
            Func<long, long, long> lop, Func<double, double, double> dop,
            Func<long, long, long> lopinv, Func<double, double, double> dopinv)
        {
            var c1 = v1.val.Count();
            if (c1 == 0)
            {
                return v2;
            }
            if (c1 == 1)
            {
                if (v2.val.Count() == 0)
                    return v1;
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
                            v2.val.Select(a => lop(k1, Convert.ToInt64(a))).Cast<object>()
                        };
                    }
                    else
                    {
                        double k1 = Convert.ToDouble(k);
                        return new Result()
                        {
                            type = ResultType.Double,
                            val =
                            v2.val.Select(a => dop(k1, Convert.ToDouble(a))).Cast<object>()
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
                        v2.val.Select(a => dop(k1, Convert.ToDouble(a))).Cast<object>()
                    };
                }
            }
            if (v2.val.Count() <= 1)
            {
                return oper(v2, v1, lopinv, dopinv, lop, dop);
            }
            throw new SCException("不支持多个元素与多个元素之间的运算");
        }

        ///////////////////////////////////////

        /// <summary>
        /// N个参数
        /// </summary>
        /// <param name="conv"></param>
        /// <returns></returns>
        public Result parn(Func<Result, Result> conv)
        {
            return conv(this);
        }

        /// <summary>
        /// N个参数
        /// </summary>
        /// <param name="count"></param>
        /// <param name="conv"></param>
        /// <returns></returns>
        public Result parn(int count, Func<Result, Result> conv)
        {
            if (val.Count() != count)
                throw new SCException($"只接受{count}个参数");
            return conv(this);
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
        /// Range
        /// </summary>
        /// <param name="conv"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Result par2range()
        {
            if (val.Count() != 2)
                throw new SCException("必须有两个参数");
            var l = new List<long>();
            var x = Convert.ToInt64(val.First());
            var y = Convert.ToInt64(val.Last());
            for (var i = x; i <= y; i++)
            {
                l.Add(x);
            }
            return new Result() { val = new LazyRange(Convert.ToInt64(val.First()), Convert.ToInt64(val.Last())) };
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

    public class StringResult : Result
    {
        private string desc;

        public StringResult(string desc)
        {
            this.desc = desc;
        }

        public StringResult(Result result)
        {
            this.type = result.type;
            this.val = result.val;
            this.desc = result.GetTypeString();
        }

        public override string ToString()
        {
            return desc;
        }

        public override void ToString(TextWriter output)
        {
            output.Write(desc);
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