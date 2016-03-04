using SimpleConsole.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public bool IsEmpty => !Val.Any();

        public Result()
        {

        }

        public Result(IList<Result> evalArgs)
        {
            var t = (from x in evalArgs
                    group x by x.Type into g
                    select new { type = g.Key, count = g.Count() }).ToList();
            switch (t.Count)
            {
                case 0:
                    return;
                case 1:
                    Type = t.Single().type;
                    Val = evalArgs.Select(a => a.Val).Aggregate((a, b) => a.Concat(b));
                    break;
                default:
                    Type = t.Max(a => a.type);
                    Val = evalArgs.Select(a => a.Cast(Type).Val).Aggregate((a, b) => a.Concat(b));
                    break;
            }
        }

        /// <summary>
        /// 类型
        /// </summary>
        public ResultType Type { set; get; } = ResultType.Long;

        /// <summary>
        /// 变量值
        /// </summary>
        public IEnumerable<object> Val { set; get; } = Enumerable.Empty<object>();

        public bool Bool()
        {
            return Convert.ToBoolean(Val.First());
        }

        private IList<long> CastLong()
        {
            return CastVal(ResultType.Long).Select(a => (long)a).ToList();
        }

        private IList<double> CastDouble()
        {
            return CastVal(ResultType.Double).Select(a => (double)a).ToList();
        }

        private Result Cast(ResultType type)
        {
            if (Type == type)
                return this;
            switch (type)
            {
                case ResultType.Long:
                    return new Result() { Type = type, Val = Val.Select(Convert.ToInt64).Cast<object>() };
                case ResultType.Double:
                    return new Result() { Type = type, Val = Val.Select(Convert.ToDouble).Cast<object>() };
            }
            return Empty;
        }

        private IEnumerable<object> CastVal(ResultType type)
        {
            if (Type == type)
                return Val;
            switch (type)
            {
                case ResultType.Long:
                    return Val.Select(Convert.ToInt64).Cast<object>();
                case ResultType.Double:
                    return Val.Select(Convert.ToDouble).Cast<object>();
            }
            return Enumerable.Empty<object>();
        }

        public Result Compare(CompareType cmp)
        {
            bool r;
            switch (Type)
            {
                case ResultType.Long:
                    {
                        var l = CastLong();
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
                                throw new ScException("未知的比较运算符");
                        }
                        return new Result() { Val = new List<object> { r } };
                    }
                case ResultType.Double:
                    {
                        var l = CastDouble();
                        var x = l.First();
                        var y = l.Last();
                        switch (cmp)
                        {
                            case CompareType.Equal:
                                r = Math.Abs(x - y) < double.Epsilon;
                                break;
                            case CompareType.NotEqual:
                                r = Math.Abs(x - y) > double.Epsilon;
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
                                throw new ScException("未知的比较运算符");
                        }
                        return new Result() { Val = new List<object> { r } };
                    }
                default:
                    throw new ScException("未知的比较数据类型");
            }
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", Val ?? Enumerable.Empty<object>())}]";
        }

        public virtual void ToString(TextWriter output)
        {
            output.Write("[");
            ", ".Join(Val, output);
            output.Write("[");
        }

        public string GetTypeString()
        {
            return $"{string.Concat(Val.GetType().Name.TakeWhile(char.IsLetter))} :: {Type}";
        }

        public static Result operator +(Result v1, Result v2)
        {
            return Oper(v1, v2, (a, b) => a + b, (a, b) => a + b, (b, a) => a + b, (b, a) => a + b);
        }

        public static Result operator -(Result v1, Result v2)
        {
            return Oper(v1, v2, (a, b) => a - b, (a, b) => a - b, (b, a) => a - b, (b, a) => a - b);
        }

        public static Result operator *(Result v1, Result v2)
        {
            return Oper(v1, v2, (a, b) => a * b, (a, b) => a * b, (b, a) => a * b, (b, a) => a * b);
        }

        public static Result operator /(Result v1, Result v2)
        {
            return Oper(v1, v2, (a, b) => a / b, (a, b) => a / b, (b, a) => a / b, (b, a) => a / b);
        }

        public static Result operator %(Result v1, Result v2)
        {
            return Oper(v1, v2, (a, b) => a % b, (a, b) => a % b, (b, a) => a % b, (b, a) => a % b);
        }

        private static Result Oper(Result v1, Result v2,
            Func<long, long, long> lop, Func<double, double, double> dop,
            Func<long, long, long> lopinv, Func<double, double, double> dopinv)
        {
            var c1 = v1.Val.Count();
            if (c1 == 0)
            {
                return v2;
            }
            if (c1 == 1)
            {
                if (!v2.Val.Any())
                    return v1;
                var k = v1.Val.Single();
                if (v1.Type == v2.Type)
                {
                    if (v1.Type == ResultType.Long)
                    {
                        var k1 = Convert.ToInt64(k);
                        return new Result()
                        {
                            Type = ResultType.Long,
                            Val =
                            v2.Val.Select(a => lop(k1, Convert.ToInt64(a))).Cast<object>()
                        };
                    }
                    else
                    {
                        var k1 = Convert.ToDouble(k);
                        return new Result()
                        {
                            Type = ResultType.Double,
                            Val =
                            v2.Val.Select(a => dop(k1, Convert.ToDouble(a))).Cast<object>()
                        };
                    }
                }
                else
                {
                    var k1 = Convert.ToDouble(k);
                    return new Result()
                    {
                        Type = ResultType.Double,
                        Val =
                        v2.Val.Select(a => dop(k1, Convert.ToDouble(a))).Cast<object>()
                    };
                }
            }
            if (v2.Val.Count() <= 1)
            {
                return Oper(v2, v1, lopinv, dopinv, lop, dop);
            }
            throw new ScException("不支持多个元素与多个元素之间的运算");
        }

        ///////////////////////////////////////

        /// <summary>
        /// N个参数
        /// </summary>
        /// <param name="conv"></param>
        /// <returns></returns>
        public Result Parn(Func<Result, Result> conv)
        {
            return conv(this);
        }

        /// <summary>
        /// N个参数
        /// </summary>
        /// <param name="count"></param>
        /// <param name="conv"></param>
        /// <returns></returns>
        public Result Parn(int count, Func<Result, Result> conv)
        {
            if (Val.Count() != count)
                throw new ScException($"只接受{count}个参数");
            return conv(this);
        }

        /// <summary>
        /// 向量化运算 - 强制转换
        /// </summary>
        /// <param name="conv"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Result Par1(Func<object, object> conv, ResultType type)
        {
            return new Result() { Type = type, Val = Val.Select(conv) };
        }

        /// <summary>
        /// 向量化运算 - 动态转换
        /// </summary>
        /// <param name="conv1"></param>
        /// <param name="conv2"></param>
        /// <returns></returns>
        public Result Par1A(Func<long, long> conv1, Func<double, double> conv2)
        {            
            switch (Type)
            {
                case ResultType.Long:
                    return new Result() { Type = Type, Val = CastLong().Select(conv1).Cast<object>() };
                case ResultType.Double:
                    return new Result() { Type = Type, Val = CastDouble().Select(conv2).Cast<object>() };
                default:
                    return Empty;
            }
        }

        /// <summary>
        /// 两个参数
        /// </summary>
        /// <param name="conv"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Result Par2(Func<object, object, object> conv, ResultType type)
        {
            if (Val.Count() != 2)
                throw new ScException("必须有两个参数");
            return new Result() { Type = type, Val = new List<object>() { conv(Val.First(), Val.Last()) } };
        }

        /// <summary>
        /// Range
        /// </summary>
        /// <returns></returns>
        public Result Par2Range()
        {
            if (Val.Count() != 2)
                throw new ScException("必须有两个参数");
            var l = new List<long>();
            var x = Convert.ToInt64(Val.First());
            var y = Convert.ToInt64(Val.Last());
            for (var i = x; i <= y; i++)
            {
                l.Add(x);
            }
            return new Result() { Val = new LazyRange(Convert.ToInt64(Val.First()), Convert.ToInt64(Val.Last())) };
        }

        /// <summary>
        /// 归约
        /// </summary>
        /// <param name="conv1"></param>
        /// <param name="conv2"></param>
        /// <returns></returns>
        public Result Par2A(Func<long, long, long> conv1, Func<double, double, double> conv2)
        {
            switch (Type)
            {
                case ResultType.Long:
                    return new Result() { Type = Type, Val = new List<object>() { CastLong().Aggregate(conv1) } };
                case ResultType.Double:
                    return new Result() { Type = Type, Val = new List<object>() { CastDouble().Aggregate(conv2) } };
                default:
                    return Empty;
            }
        }
    }

    public class StringResult : Result
    {
        private readonly string _desc;

        public StringResult(string desc)
        {
            _desc = desc;
        }

        public override string ToString()
        {
            return _desc;
        }

        public override void ToString(TextWriter output)
        {
            output.Write(_desc);
        }
    }
}