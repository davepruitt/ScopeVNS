using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScopeVNS
{
    public static class PicoScopeTypeConverter
    {
        public static PicoScopeType ConvertToEnumeratedType(string description)
        {
            var type = typeof(PicoScopeType);

            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (PicoScopeType)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (PicoScopeType)field.GetValue(null);
                }
            }

            return PicoScopeType.Unknown;
        }

        public static string ConvertToStringDescription(PicoScopeType enumerated_type)
        {
            FieldInfo fi = enumerated_type.GetType().GetField(enumerated_type.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return enumerated_type.ToString();
        }
    }
}
