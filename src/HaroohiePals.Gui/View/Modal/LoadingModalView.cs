using ImGuiNET;
using System.Numerics;

namespace HaroohiePals.Gui.View.Modal;

public class LoadingModalView : ModalView
{
    private const float SPINNER_TEXT_MARGIN = 16f;
    private const float SPINNER_RADIUS = 12f;
    private const int SPINNER_THICKNESS = 2;
    private const string SPINNER_LABEL = "##loading_spinner";

    private const float SIZE_X_MARGIN = 30f;

    private const string DEFAULT_DESCRIPTION = "Please wait...";
    private const string DEFAULT_TITLE = "Loading";
    private const bool SHOW_MODAL_CLOSE_BUTTON = false;

    private static readonly Vector2 DefaultSize = new Vector2(160, 80);

    private string _description;

    public LoadingModalView() 
        : this(DEFAULT_DESCRIPTION) { }

    public LoadingModalView(string description) 
        : this(description, DEFAULT_TITLE, DefaultSize) { }

    public LoadingModalView(string description, string title, Vector2 size) 
        : base(title, size, SHOW_MODAL_CLOSE_BUTTON)
    {
        _description = description;
        Size = new Vector2(GetSpinnerAndTextWidth() + SIZE_X_MARGIN, Size.Y);
    }

    protected override void DrawContent()
    {
        var avail = ImGui.GetContentRegionMax();

        float x = avail.X / 2 - GetSpinnerAndTextWidth() / 2;
        float y = avail.Y / 2;

        ImGui.SetCursorPos(new Vector2(x, y));
        ImGuiEx.Spinner(SPINNER_LABEL, SPINNER_RADIUS, SPINNER_THICKNESS, ImGui.GetColorU32(ImGuiCol.ButtonHovered));

        x += SPINNER_RADIUS * 2 + SPINNER_TEXT_MARGIN;
        y = avail.Y / 2 + ImGui.CalcTextSize(_description).Y / 2;
        ImGui.SetCursorPos(new Vector2(x, y));

        ImGui.Text(_description);
    }

    private float GetSpinnerAndTextWidth()
    {
        var textSize = ImGui.CalcTextSize(_description);
        return textSize.X + SPINNER_TEXT_MARGIN + SPINNER_RADIUS * 2;
    }
}
