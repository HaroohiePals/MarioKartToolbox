using OpenTK.Windowing.Common;

namespace HaroohiePals.Gui.View;

public interface IView
{
    bool Draw();
    void Update(UpdateArgs args) { }
}