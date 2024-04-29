using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData;

public interface IMkdsMapDataValidatorFactory
{
    IValidator<MkdsMapData> CreateMkdsMapDataValidator();
}
