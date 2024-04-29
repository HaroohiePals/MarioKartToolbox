using HaroohiePals.Graphics;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;
using Vector3d = OpenTK.Mathematics.Vector3d;

namespace HaroohiePals.Nitro.G3
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GeStateUniform
    {
        public const uint ShininessFlag = (1 << 0);
        public const uint AlphaPassFlag = (1 << 1);

        public       uint                    PolygonAttr;
        public       uint                    TexImageParam;
        public       uint                    Flags;
        private      uint                    _pad0;
        public       System.Numerics.Vector4 Diffuse;
        public       System.Numerics.Vector4 Ambient;
        public       System.Numerics.Vector4 Specular;
        public       System.Numerics.Vector4 Emission;
        public fixed float                   LightVectors[4 * 4];
        public fixed float                   LightColors[4 * 4];
        public fixed float                   PosMtxStack[16 * 31]; //index 31 is the current matrix
        public fixed float                   PosMtx[4 * 4];
        public fixed float                   DirMtxStack[16 * 31];
        public fixed float                   DirMtx[4 * 4];
        public fixed float                   TexMtx[4 * 4];

        public void SetLightColor(int id, Rgb555 color)
        {
            LightColors[id * 4 + 0] = color.R / 31f;
            LightColors[id * 4 + 1] = color.G / 31f;
            LightColors[id * 4 + 2] = color.B / 31f;
            LightColors[id * 4 + 3] = 1f;
        }

        public void SetLightVector(int id, in Vector3d vector)
        {
            LightVectors[id * 4 + 0] = (float)vector.X;
            LightVectors[id * 4 + 1] = (float)vector.Y;
            LightVectors[id * 4 + 2] = (float)vector.Z;
            LightVectors[id * 4 + 3] = 0;
        }

        public void SetPosStackMtx(int id, in Matrix4d matrix)
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    PosMtxStack[id * 16 + i * 4 + j] = (float)matrix[i, j];
        }

        public void SetPosMtx(in Matrix4d matrix)
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    PosMtx[i * 4 + j] = (float)matrix[i, j];
        }

        public void SetDirMtx(in Matrix3d matrix)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    DirMtx[i * 4 + j] = (float)matrix[i, j];
            DirMtx[0 * 4 + 3] = 0;
            DirMtx[1 * 4 + 3] = 0;
            DirMtx[2 * 4 + 3] = 0;
            DirMtx[3 * 4 + 0] = 0;
            DirMtx[3 * 4 + 1] = 0;
            DirMtx[3 * 4 + 2] = 0;
            DirMtx[3 * 4 + 3] = 1;
        }

        public void SetTexMtx(in Matrix4d matrix)
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    TexMtx[i * 4 + j] = (float)matrix[i, j];
        }

        public void SetDirStackMtx(int id, in Matrix3d matrix)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    DirMtxStack[id * 16 + i * 4 + j] = (float)matrix[i, j];
            DirMtxStack[id * 16 + 0 * 4 + 3] = 0;
            DirMtxStack[id * 16 + 1 * 4 + 3] = 0;
            DirMtxStack[id * 16 + 2 * 4 + 3] = 0;
            DirMtxStack[id * 16 + 3 * 4 + 0] = 0;
            DirMtxStack[id * 16 + 3 * 4 + 1] = 0;
            DirMtxStack[id * 16 + 3 * 4 + 2] = 0;
            DirMtxStack[id * 16 + 3 * 4 + 3] = 1;
        }

        public static int Size => Marshal.SizeOf<GeStateUniform>();
    }
}