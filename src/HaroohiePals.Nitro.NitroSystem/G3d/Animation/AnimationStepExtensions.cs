using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Animation;

public static class AnimationStepExtensions
{
    public static int GetStepSize(this AnimationStep step) => step switch
    {
        AnimationStep.Step1 => 1,
        AnimationStep.Step2 => 2,
        AnimationStep.Step4 => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(step), step, null)
    };
}
