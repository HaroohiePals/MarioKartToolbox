using HaroohiePals.MarioKart.MapData;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

internal class MkdsPointWifiCoordOutOfRangeValidationRule : IValidationRule<IPoint>
{
    private readonly string _parentRuleName = string.Empty;
    public string Name => _parentRuleName;

    public MkdsPointWifiCoordOutOfRangeValidationRule(string parentRuleName)
    {
        _parentRuleName = parentRuleName;
    }

    public IReadOnlyList<ValidationError> Validate(IPoint obj)
    {
        var errors = new List<ValidationError>();

        if (Math.Abs(obj.Position.X) > 4096.0 || Math.Abs(obj.Position.Y) > 4096.0 || Math.Abs(obj.Position.Z) > 4096.0)
            errors.Add(new MkdsPointWifiCoordOutOfRangeValidationError(this, obj));

        return errors;
    }
}
