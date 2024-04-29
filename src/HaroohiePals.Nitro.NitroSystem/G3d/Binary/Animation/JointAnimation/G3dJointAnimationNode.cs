using HaroohiePals.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.JointAnimation;

public sealed class G3dJointAnimationNode
{
    public const uint FLAGS_IDENTITY = 0x00000001;
    public const uint FLAGS_IDENTITY_TRANSLATION = 0x00000002;
    public const uint FLAGS_BASE_TRANSLATION = 0x00000004;
    public const uint FLAGS_CONST_TRANSLATION_X = 0x00000008;
    public const uint FLAGS_CONST_TRANSLATION_Y = 0x00000010;
    public const uint FLAGS_CONST_TRANSLATION_Z = 0x00000020;
    public const uint FLAGS_IDENTITY_ROTATION = 0x00000040;
    public const uint FLAGS_BASE_ROTATION = 0x00000080;
    public const uint FLAGS_CONST_ROTATION = 0x00000100;
    public const uint FLAGS_IDENTITY_SCALE = 0x00000200;
    public const uint FLAGS_BASE_SCALE = 0x00000400;
    public const uint FLAGS_CONST_SCALE_X = 0x00000800;
    public const uint FLAGS_CONST_SCALE_Y = 0x00001000;
    public const uint FLAGS_CONST_SCALE_Z = 0x00002000;
    public const uint FLAGS_NODE_MASK = 0xFF000000;
    public const int FLAGS_NODE_SHIFT = 24;

    public G3dJointAnimationNode(EndianBinaryReaderEx reader, int nrFrames)
    {
        Flags = reader.Read<uint>();
        if ((Flags & FLAGS_IDENTITY) != 0)
        {
            return;
        }

        if ((Flags & FLAGS_IDENTITY_TRANSLATION) == 0 && (Flags & FLAGS_BASE_TRANSLATION) == 0)
        {
            TranslationX = new G3dJointAnimationTranslation(reader, (Flags & FLAGS_CONST_TRANSLATION_X) != 0, nrFrames);
            TranslationY = new G3dJointAnimationTranslation(reader, (Flags & FLAGS_CONST_TRANSLATION_Y) != 0, nrFrames);
            TranslationZ = new G3dJointAnimationTranslation(reader, (Flags & FLAGS_CONST_TRANSLATION_Z) != 0, nrFrames);
        }

        if ((Flags & FLAGS_IDENTITY_ROTATION) == 0 && (Flags & FLAGS_BASE_ROTATION) == 0)
        {
            Rotation = new G3dJointAnimationRotation(reader, (Flags & FLAGS_CONST_ROTATION) != 0, nrFrames);
        }

        if ((Flags & FLAGS_IDENTITY_SCALE) == 0 && (Flags & FLAGS_BASE_SCALE) == 0)
        {
            ScaleX = new G3dJointAnimationScale(reader, (Flags & FLAGS_CONST_SCALE_X) != 0, nrFrames);
            ScaleY = new G3dJointAnimationScale(reader, (Flags & FLAGS_CONST_SCALE_Y) != 0, nrFrames);
            ScaleZ = new G3dJointAnimationScale(reader, (Flags & FLAGS_CONST_SCALE_Z) != 0, nrFrames);
        }
    }

    public uint Flags;

    public G3dJointAnimationTranslation TranslationX;
    public G3dJointAnimationTranslation TranslationY;
    public G3dJointAnimationTranslation TranslationZ;

    public G3dJointAnimationRotation Rotation;

    public G3dJointAnimationScale ScaleX;
    public G3dJointAnimationScale ScaleY;
    public G3dJointAnimationScale ScaleZ;
}
