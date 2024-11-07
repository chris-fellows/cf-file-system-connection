﻿using System.ComponentModel;

namespace CFFileSystemMobile.Utilities
{
    internal class EnumUtilities
    {
        public static string GetEnumDescription(Enum value)
        {
            // variables  
            var enumType = value.GetType();
            var field = enumType.GetField(value.ToString());
            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);

            // return  
            return attributes.Length == 0 ? value.ToString() : ((DescriptionAttribute)attributes[0]).Description;
        }
    }
}
