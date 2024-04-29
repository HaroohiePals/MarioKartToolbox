#nullable enable

using System.Collections.Generic;

namespace HaroohiePals.Gui.View.Modal;

public interface IModalService
{
    bool IsAnyModalOpen { get; }
    void ShowModal(ModalView modal);
    void HideModal(ModalView modal);
    IReadOnlyCollection<ModalView> GetAllModals();
}
