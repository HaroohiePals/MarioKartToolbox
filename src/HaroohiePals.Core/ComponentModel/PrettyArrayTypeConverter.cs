using System;
using System.ComponentModel;
using System.Globalization;

namespace HaroohiePals.Core.ComponentModel
{
    public class PrettyArrayTypeConverter : ArrayConverter
    {
        public override object ConvertTo(
            ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
        {
            return "";
        }
    }
}