using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HaroohiePals.NitroKart.MapData.Intermediate.ComponentModel
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ListViewColumnAttribute : Attribute
    {
        public int      Order { get; }
        public string[] Names { get; }

        public ListViewColumnAttribute(int order, params string[] names)
        {
            Order = order;
            Names = names;
        }

        private static IEnumerable<(MemberInfo memberInfo, ListViewColumnAttribute attribute)> GetColumns(Type type)
            => type.GetMembers()
                .Select(m => (m, m.GetCustomAttribute<ListViewColumnAttribute>()))
                .Where(m => m.Item2 != null)
                .OrderBy(m => m.Item2.Order);

        // public static void SetupListViewHeaders(ListView.ColumnHeaderCollection headers, Type type)
        // {
        //     headers.Clear();
        //     headers.Add(new ColumnHeader() { Text = "#", Width = 35 });
        //     foreach (var (_, attribute) in GetColumns(type))
        //         foreach (string name in attribute.Names)
        //             headers.Add(name);
        // }
        //
        // public static ListViewItem GetListViewItem(object obj, int index)
        // {
        //     var item = new ListViewItem("" + index);
        //     foreach (var (memberInfo, attribute) in GetColumns(obj.GetType()))
        //     {
        //         object value;
        //         Type   type;
        //         if (memberInfo is PropertyInfo prop)
        //         {
        //             value = prop.GetValue(obj);
        //             type  = prop.PropertyType;
        //         }
        //         else if (memberInfo is FieldInfo field)
        //         {
        //             value = field.GetValue(obj);
        //             type  = field.FieldType;
        //         }
        //         else
        //             throw new Exception("Attribute applied on invalid member type");
        //
        //         if (type.IsArray)
        //         {
        //             for (int i = 0; i < attribute.Names.Length; i++)
        //             {
        //                 if (value != null && i < ((Array) value).Length)
        //                     item.SubItems.Add(((Array) value).GetValue(i).ToString());
        //                 else
        //                     item.SubItems.Add("");
        //             }
        //         }
        //         else if (type == typeof(MobjSettings) || type.IsSubclassOf(typeof(MobjSettings)))
        //         {
        //             var mobjSettings = (MobjSettings) value;
        //             for (int i = 0; i < attribute.Names.Length; i++)
        //             {
        //                 if (mobjSettings != null && i < mobjSettings.Settings.Length)
        //                     item.SubItems.Add(mobjSettings.Settings[i].ToString());
        //                 else
        //                     item.SubItems.Add("");
        //             }
        //         }
        //         else if (type == typeof(Vector3))
        //         {
        //             if (attribute.Names.Length != 3)
        //                 throw new Exception("Invalid number of column headers for Vector3");
        //             for (int i = 0; i < 3; i++)
        //                 item.SubItems.Add(((Vector3)value)[i].ToString());
        //         }
        //         else if (type == typeof(Vector2))
        //         {
        //             if (attribute.Names.Length != 2)
        //                 throw new Exception("Invalid number of column headers for Vector2");
        //             for (int i = 0; i < 2; i++)
        //                 item.SubItems.Add(((Vector2)value)[i].ToString());
        //         }
        //         else if (value == null)
        //             item.SubItems.Add("");
        //         else
        //             item.SubItems.Add(value.ToString());
        //     }
        //
        //     return item;
        // }
    }
}