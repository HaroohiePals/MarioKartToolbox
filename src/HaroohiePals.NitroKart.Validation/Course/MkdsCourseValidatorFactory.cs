using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapObj;
using HaroohiePals.NitroKart.Validation.MapData;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.Course;

public class MkdsCourseValidatorFactory : IMkdsCourseValidatorFactory
{
    private readonly IValidator<MkdsMapData> _mapDataValidator;
    private readonly IMkdsMapObjDatabase _mapObjDatabase;

    public MkdsCourseValidatorFactory(IMkdsMapDataValidatorFactory mapDataValidatorFactory, IMkdsMapObjDatabase mapObjDatabase)
    {
        _mapDataValidator = mapDataValidatorFactory.CreateMkdsMapDataValidator();
        _mapObjDatabase = mapObjDatabase;
    }

    public IValidator<IMkdsCourse> CreateMkdsCourseValidator()
    {
        var validator = new Validator<IMkdsCourse>();

        validator.Rules.Add(new MkdsCourseMapDataValidationRule(_mapDataValidator));
        validator.Rules.Add(new MkdsRequiredMapObjFilesValidationRule(_mapObjDatabase));

        return validator;
    }
}
