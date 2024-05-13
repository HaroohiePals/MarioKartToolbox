using ImGuiNET;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace HaroohiePals.Gui;

public static class ImGuiEx
{
    private const float BASE_FONT_SIZE = 15f;

    public static bool IsScalarType(TypeCode type) => type switch
    {
        TypeCode.Byte => true,
        TypeCode.SByte => true,
        TypeCode.UInt16 => true,
        TypeCode.Int16 => true,
        TypeCode.UInt32 => true,
        TypeCode.Int32 => true,
        TypeCode.UInt64 => true,
        TypeCode.Int64 => true,
        TypeCode.Single => true,
        TypeCode.Double => true,
        _ => false
    };

    public static bool IsScalarType(Type type) => IsScalarType(Type.GetTypeCode(type));

    public static unsafe bool DragScalar<T>(string label, T value, out T output, string format = null, T? min = null, T? max = null, float speed = 1) 
        where T : unmanaged, INumber<T>
    {
        T realMin = min.HasValue ? min.Value : default;
        T realMax = max.HasValue ? max.Value : default;

        bool result = ImGui.DragScalar(label, ToImGuiDataType(typeof(T)), (IntPtr)(&value), speed, (IntPtr)(&realMin), (IntPtr)(&realMax), format);
        
        if (result)
        {
            if (min.HasValue && value < realMin)
                value = realMin;
            if (max.HasValue && value > realMax)
                value = realMax;
        }

        output = value;

        return result;
    }

    public static bool DragScalar(string label, object value, out object output, string format = null, object min = null, object max = null, float speed = 1)
    {
        bool result = false;

        Type type = value.GetType();

        output = 0;

        switch (value)
        {
            case byte n:
                result = DragScalar(label, n, out n, format,
                    min != null ? (byte)Convert.ChangeType(min, type) : null,
                    max != null ? (byte)Convert.ChangeType(max, type) : null, speed);
                output = n;
                break;
            case sbyte n:
                result = DragScalar(label, n, out n, format,
                    min != null ? (sbyte)Convert.ChangeType(min, type) : null,
                    max != null ? (sbyte)Convert.ChangeType(max, type) : null, speed);
                output = n;
                break;
            case ushort n:
                result = DragScalar(label, n, out n, format,
                    min != null ? (ushort)Convert.ChangeType(min, type) : null,
                    max != null ? (ushort)Convert.ChangeType(max, type) : null, speed);
                output = n;
                break;
            case short n:
                result = DragScalar(label, n, out n, format,
                     min != null ? (short)Convert.ChangeType(min, type) : null,
                     max != null ? (short)Convert.ChangeType(max, type) : null, speed);
                output = n;
                break;
            case uint n:
                result = DragScalar(label, n, out n, format,
                    min != null ? (uint)Convert.ChangeType(min, type) : null,
                    max != null ? (uint)Convert.ChangeType(max, type) : null, speed);
                output = n;
                break;
            case int n:
                result = DragScalar(label, n, out n, format,
                    min != null ? (int)Convert.ChangeType(min, type) : null,
                    max != null ? (int)Convert.ChangeType(max, type) : null, speed);
                output = n;
                break;
            case ulong n:
                result = DragScalar(label, n, out n, format,
                    min != null ? (ulong)Convert.ChangeType(min, type) : null,
                    max != null ? (ulong)Convert.ChangeType(max, type) : null, speed);
                output = n;
                break;
            case long n:
                result = DragScalar(label, n, out n, format,
                    min != null ? (long)Convert.ChangeType(min, type) : null,
                    max != null ? (long)Convert.ChangeType(max, type) : null, speed);
                output = n;
                break;
            case float n:
                result = DragScalar(label, n, out n, format,
                    min != null ? (float)Convert.ChangeType(min, type) : null,
                    max != null ? (float)Convert.ChangeType(max, type) : null, speed);
                output = n;
                break;
            case double n:
                result = DragScalar(label, n, out n, format,
                    min != null ? (double)Convert.ChangeType(min, type) : null,
                    max != null ? (double)Convert.ChangeType(max, type) : null, speed);
                output = n;
                break;
            default:

                break;
        }

        return result;
    }

    public static bool CheckboxFlags<TEnum>(string label, ref TEnum flags, TEnum flagValue) where TEnum : Enum
    {
        int flagsInt = (int)Convert.ChangeType(flags, typeof(int));
        int flagValueInt = (int)Convert.ChangeType(flagValue, typeof(int));

        if (ImGui.CheckboxFlags(label, ref flagsInt, flagValueInt))
        {
            flags = (TEnum)Convert.ChangeType(flagsInt, typeof(int));
            return true;
        }
        return false;
    }

    public static bool ComboEnum<TEnum>(string label, ref TEnum value, int? overrideIndex = null) where TEnum : Enum
    {
        var type = value.GetType();

        string[] names = Enum.GetNames(value.GetType());
        var values = Enum.GetValues(value.GetType());

        int index = overrideIndex.HasValue ? overrideIndex.Value : Array.IndexOf(values, value);

        if (ImGui.Combo(label, ref index, names, names.Length))
        {
            value = (TEnum)values.GetValue(index);
            return true;
        }
        return false;
    }

    public static bool ComboEnum(string label, ref object value, int? overrideIndex = null)
    {
        var type = value.GetType();

        string[] names = Enum.GetNames(value.GetType());
        var values = Enum.GetValues(value.GetType());

        int index = overrideIndex.HasValue ? overrideIndex.Value : Array.IndexOf(values, value);

        if (ImGui.Combo(label, ref index, names, names.Length))
        {
            value = Convert.ChangeType(values.GetValue(index), type);
            return true;
        }
        return false;
    }

    public static bool AreKeysDown(params ImGuiKey[] keys)
    {
        var keysDown = ImGui.GetIO().KeysDown;

        if (keys.Length == 0)
            return false;

        foreach (ImGuiKey key in keys)
        {
            if (!keysDown[(int)key])
                return false;
        }    

        return true;
    }

    // https://github.com/ocornut/imgui/issues/1901
    public static bool Spinner(string label, float radius, int thickness, uint color)
    {
        var style = ImGui.GetStyle();
        var id = ImGui.GetID(label);

        var pos = ImGui.GetCursorPos() + ImGui.GetWindowPos();
        var size = new Vector2(radius * 2, (radius + style.FramePadding.Y) * 2);

        //Rectangle
        var bb = new ImRect { Min = pos, Max = pos + size };
        igItemSize_Rect(bb, style.FramePadding.Y);
        if (!igItemAdd(bb, id))
            return false;

        // Render
        ImGui.GetWindowDrawList().PathClear();

        int numSegments = 30;
        int start = (int)Math.Abs(Math.Sin(ImGui.GetTime() * 1.8f) * (numSegments - 5));

        float a_min = (float)(Math.PI * 2.0f * ((float)start) / (float)numSegments);
        float a_max = (float)(Math.PI * 2.0f * ((float)numSegments - 3) / (float)numSegments);

        var centre = new Vector2(pos.X + radius, pos.Y + radius + style.FramePadding.Y);

        for (int i = 0; i < numSegments; i++)
        {
            float a = a_min + ((float)i / (float)numSegments) * (a_max - a_min);
            ImGui.GetWindowDrawList().PathLineTo(new Vector2((float)(centre.X + Math.Cos(a + ImGui.GetTime() * 8) * radius), (float)(centre.Y + Math.Sin(a + ImGui.GetTime() * 8) * radius)));
        }

        ImGui.GetWindowDrawList().PathStroke(color, ImDrawFlags.None, thickness);

        return true;
    }

    public static float GetUiScale() => ImGui.GetFontSize() / BASE_FONT_SIZE;

    public static float CalcUiScaledValue(float value) => value * GetUiScale();

    public static Vector4 ColorConvertColorToFloat4(Color color) 
        => new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);

    public static Color ColorConvertFloat4ToColor(Vector4 vec) 
        => Color.FromArgb((int)(vec.W * 255f), (int)(vec.X * 255f), (int)(vec.Y * 255f), (int)(vec.Z * 255f));

    public static uint ColorConvertColorToU32(Color color) 
        => ImGui.ColorConvertFloat4ToU32(ColorConvertColorToFloat4(color));

    public static Color ColorConvertU32ToColor(uint color)
        => ColorConvertFloat4ToColor(ImGui.ColorConvertU32ToFloat4(color));

    public static Color GetColor(ImGuiCol idx)
        => ColorConvertU32ToColor(ImGui.GetColorU32(idx));

    private static ImGuiDataType ToImGuiDataType(TypeCode type) => type switch
    {
        TypeCode.Byte => ImGuiDataType.U8,
        TypeCode.SByte => ImGuiDataType.S8,
        TypeCode.UInt16 => ImGuiDataType.U16,
        TypeCode.Int16 => ImGuiDataType.S16,
        TypeCode.UInt32 => ImGuiDataType.U32,
        TypeCode.Int32 => ImGuiDataType.S32,
        TypeCode.UInt64 => ImGuiDataType.U64,
        TypeCode.Int64 => ImGuiDataType.S64,
        TypeCode.Single => ImGuiDataType.Float,
        TypeCode.Double => ImGuiDataType.Double,

        _ => throw new NotImplementedException()
    };

    private static ImGuiDataType ToImGuiDataType(Type type) => ToImGuiDataType(Type.GetTypeCode(type));

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    private static extern void igItemSize_Rect(ImRect bb, float textBaselineY);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool igItemAdd(ImRect bb, uint id);

}
