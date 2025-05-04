namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

class CameraPreviewSettings
{
    public CameraPreviewViewportMode ViewportMode = CameraPreviewViewportMode.DualScreen;
    public CameraPreviewMode Mode = CameraPreviewMode.AnimIntro;

    public bool UseScreenGap = true;
    public bool UseNativeResolution = false;

    public bool ReplaySimulateCountdown = true;
    public bool ReplayMoveDriver = true;
    public float ReplayDriverMoveSpeed = 5f;
    public bool ReplayApplyDriverPosOnKtp2 = false;

    public bool EditSecondTarget = false;
    public bool EditKeepOriginalTargetDistance = false;
    public float EditTargetDistance = 1500f;
}