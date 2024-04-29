#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.Gui.View.Modal;

public class ModalService : IModalService
{
    private readonly List<ModalView> _modalViews = [];
    public bool IsAnyModalOpen => _modalViews.Any(x => x.IsOpen);

    public IReadOnlyCollection<ModalView> GetAllModals() => _modalViews;

    public void HideModal(ModalView modal)
    {
        _modalViews.Remove(modal);

        modal.Close();
    }

    public void ShowModal(ModalView modal)
    {
        if (!_modalViews.Contains(modal))
            _modalViews.Add(modal);

        modal.Open();
    }
}
