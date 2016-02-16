using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Typing
{
    [AttributeUsage(AttributeTargets.Field)]
    class SCOperationAttribute : DescriptionAttribute
    {
        private int leftLevel;
        private int rightLevel;

        /// <summary>
        /// 左结合优先级
        /// </summary>
        public int LeftLevel
        {
            get
            {
                return leftLevel;
            }
        }

        /// <summary>
        /// 右结合优先级
        /// </summary>
        public int RightLevel
        {
            get
            {
                return rightLevel;
            }
        }

        public SCOperationAttribute()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="description">操作符</param>
        /// <param name="leftLevel">左结合优先级</param>
        /// <param name="rightLevel">右结合优先级</param>
        public SCOperationAttribute(string description, int leftLevel, int rightLevel) : base(description)
        {
            this.leftLevel = leftLevel;
            this.rightLevel = rightLevel;
        }
    }

    enum OpType
    {
        [SCOperation("=", 4, 5)]
        Equal,

        [SCOperation("+", 10, 10)]
        Add,

        [SCOperation("-", 10, 10)]
        Subtract,

        [SCOperation("*", 20, 20)]
        Multiply,

        [SCOperation("/", 20, 20)]
        Divide,

        [SCOperation("%", 20, 20)]
        Mod,

        [SCOperation()]
        Unknown,
    }

    static class OpTypeHelper
    {
        static Dictionary<string, OpType> dict = new Dictionary<string, OpType>();

        static OpTypeHelper()
        {
            foreach (OpType item in Enum.GetValues(typeof(OpType)))
            {
                var attr = item.GetAttr();
                if (attr.Description != null)
                {
                    dict.Add(attr.Description, item);
                }
            }
        }

        public static SCOperationAttribute GetAttr(this OpType type)
        {
            return EnumHelper.GetAttrOfEnum<SCOperationAttribute, OpType>(type);
        }

        public static OpType GetTypeOfString(string str)
        {
            if (dict.ContainsKey(str))
            {
                return dict[str];
            }
            return OpType.Unknown;
        }
    }
}
