using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.Course;

public class MkdsCourseMapDataValidationRule : IValidationRule<IMkdsCourse>
{
    private readonly IValidator<MkdsMapData> _mapDataValidator;

    public string Name => "Map Data";

    public MkdsCourseMapDataValidationRule(IValidator<MkdsMapData> mapDataValidator)
    {
        _mapDataValidator = mapDataValidator;
    }

    public IReadOnlyList<ValidationError> Validate(IMkdsCourse obj)
    {
        var errors = new List<ValidationError>();
        errors.AddRange(_mapDataValidator.Validate(obj.MapData));
        return errors;
    }
}