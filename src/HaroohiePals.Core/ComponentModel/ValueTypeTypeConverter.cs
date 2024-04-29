using System;
using System.Collections;
using System.ComponentModel;

namespace HaroohiePals.Core.ComponentModel
{
    public class ValueTypeTypeConverter : ExpandableObjectConverter
    {
        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) => true;

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            if (propertyValues == null)
                throw new ArgumentNullException(nameof(propertyValues));

            object boxed = Activator.CreateInstance(context.PropertyDescriptor.PropertyType);
            foreach (DictionaryEntry entry in propertyValues)
            {
                var pi = context.PropertyDescriptor.PropertyType.GetProperty(entry.Key.ToString());
                if (pi != null && pi.CanWrite)
                    pi.SetValue(boxed, Convert.ChangeType(entry.Value, pi.PropertyType), null);
            }

            return boxed;
        }
    }
}