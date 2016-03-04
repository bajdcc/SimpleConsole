using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SimpleConsole.Typing
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class ScOperationAttribute : DescriptionAttribute
    {
        /// <summary>
        /// 左结合优先级
        /// </summary>
        public int LeftLevel { get; }

        /// <summary>
        /// 右结合优先级
        /// </summary>
        public int RightLevel { get; }

        public ScOperationAttribute()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="description">操作符</param>
        /// <param name="leftLevel">左结合优先级</param>
        /// <param name="rightLevel">右结合优先级</param>
        public ScOperationAttribute(string description, int leftLevel, int rightLevel) : base(description)
        {
            LeftLevel = leftLevel;
            RightLevel = rightLevel;
        }
    }

    internal enum OpType
    {
        [ScOperation("=", 4, 5)]
        Equal,

        [ScOperation("|", 9, 9)]
        Match,

        [ScOperation("+", 10, 10)]
        Add,

        [ScOperation("-", 10, 10)]
        Subtract,

        [ScOperation("*", 20, 20)]
        Multiply,

        [ScOperation("/", 20, 20)]
        Divide,

        [ScOperation("%", 20, 20)]
        Mod,

        [ScOperation()]
        Unknown,
    }

    internal static class OpTypeHelper
    {
        private static readonly Dictionary<string, OpType> Dict = new Dictionary<string, OpType>();

        static OpTypeHelper()
        {
            foreach (OpType item in Enum.GetValues(typeof(OpType)))
            {
                var attr = item.GetAttr();
                if (attr.Description != null)
                {
                    Dict.Add(attr.Description, item);
                }
            }
        }

        public static ScOperationAttribute GetAttr(this OpType type)
        {
            return EnumHelper.GetAttrOfEnum<ScOperationAttribute, OpType>(type);
        }

        public static OpType GetTypeOfString(string str)
        {
            if (Dict.ContainsKey(str))
            {
                return Dict[str];
            }
            return OpType.Unknown;
        }
    }
}
