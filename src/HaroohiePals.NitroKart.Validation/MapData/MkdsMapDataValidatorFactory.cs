using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapObj;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData;

public class MkdsMapDataValidatorFactory : IMkdsMapDataValidatorFactory
{
    private readonly IMkdsMapObjDatabase _mobjDatabase;

    public MkdsMapDataValidatorFactory(IMkdsMapObjDatabase mobjDatabase)
    {
        _mobjDatabase = mobjDatabase;
    }

    public IValidator<MkdsMapData> CreateMkdsMapDataValidator()
    {
        var validator = new Validator<MkdsMapData>();
        validator.Rules.Add(new MkdsMapDataValidationRule(_mobjDatabase));
        return validator;
    }
}
