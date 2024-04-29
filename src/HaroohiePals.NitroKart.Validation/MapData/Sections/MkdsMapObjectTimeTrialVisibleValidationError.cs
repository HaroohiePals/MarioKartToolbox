using HaroohiePals.Actions;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

class MkdsMapObjectTimeTrialVisibleValidationError : ValidationError
{
    private readonly bool _isTimeTrialVisible;
    private readonly MkdsMapObject _entry;

    public MkdsMapObjectTimeTrialVisibleValidationError(IValidationRule rule, MkdsMapObject source, bool isTimeTrialVisible) 
        : base(rule, ErrorLevel.Warning, $"The TT Visible Flag should be {(isTimeTrialVisible ? "checked" : "unchecked")}", source, true)
    {
        _isTimeTrialVisible = isTimeTrialVisible;
        _entry = source;
    }

    protected override IAction Fix() => _entry.SetPropertyAction(x => x.TTVisible, _isTimeTrialVisible);
}
