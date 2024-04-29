using HaroohiePals.MarioKart.Validation.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapObj;
using HaroohiePals.NitroKart.Validation.MapData.Sections;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData;

internal class MkdsMapDataValidationRule : IValidationRule<MkdsMapData>
{
    public string Name => "Map Data";

    private readonly MapDataCollectionValidationRule<MkdsMapData, MkdsArea, MkdsAreaValidationRule>
        _areaValidationRule = new();
    private readonly MapDataCollectionValidationRule<MkdsMapData, MkdsCamera, MkdsCameraValidationRule>
        _cameValidationRule = new();
    private readonly MapDataCollectionValidationRule<MkdsMapData, MkdsRespawnPoint, MkdsRespawnPointValidationRule>
        _ktpjValidationRule = new();
    private readonly MapDataCollectionValidationRule<MkdsMapData, MkdsCannonPoint, MkdsCannonPointValidationRule>
        _ktpcValidationRule = new();
    private readonly MapDataCollectionValidationRule<MkdsMapData, MkdsCheckPointPath, 
        ConnectedPathValidationRule<MkdsMapData, MkdsCheckPointPath, MkdsCheckPoint, MkdsCheckPointValidationRule>>
        _cpatValidationRule = new();
    private readonly MapDataCollectionValidationRule<MkdsMapData, MkdsMapObject, MkdsMapObjectValidationRule>
        _mobjValidationRule;

    public MkdsMapDataValidationRule(IMkdsMapObjDatabase mobjDatabase)
    {
        _mobjValidationRule = new(new MkdsMapObjectValidationRule(mobjDatabase));
    }

    public IReadOnlyList<ValidationError> Validate(MkdsMapData obj)
    {
        var errors = new List<ValidationError>();

        if (obj.Areas is not null)
            errors.AddRange(_areaValidationRule.Validate((obj, obj.Areas)));
        if (obj.Cameras is not null)
            errors.AddRange(_cameValidationRule.Validate((obj, obj.Cameras)));
        if (obj.CannonPoints is not null)
            errors.AddRange(_ktpcValidationRule.Validate((obj, obj.CannonPoints)));
        if (obj.CheckPointPaths is not null)
            errors.AddRange(_cpatValidationRule.Validate((obj, obj.CheckPointPaths)));
        if (obj.MapObjects is not null)
            errors.AddRange(_mobjValidationRule.Validate((obj, obj.MapObjects)));
        if (obj.RespawnPoints is not null)
            errors.AddRange(_ktpjValidationRule.Validate((obj, obj.RespawnPoints)));

        return errors;
    }
}
