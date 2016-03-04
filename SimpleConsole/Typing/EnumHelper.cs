using System;

namespace SimpleConsole.Typing
{
    internal static class EnumHelper
    {
        internal static TAttr GetAttrOfEnum<TAttr, TEnum>(object value)
            where TAttr : Attribute
        {
            var enumType = typeof(TEnum);
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("requires type of Enum");
            }
            var name = Enum.GetName(enumType, Convert.ToInt32(value));
            if (name == null)
                return default(TAttr);
            var objs = enumType.GetField(name).GetCustomAttributes(typeof(TAttr), false);
            if (objs.Length == 0)
            {
                return default(TAttr);
            }
            else
            {
                return objs[0] as TAttr;
            }
        }
    }
}
