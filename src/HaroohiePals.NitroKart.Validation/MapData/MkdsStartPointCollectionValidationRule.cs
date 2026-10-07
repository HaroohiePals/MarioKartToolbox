using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData;

class MkdsStartPointCollectionValidationRule : IValidationRule<MkdsMapData>
{
    public const int MG_START_POINT_COUNT = 8;

    public string Name => "Start Points";

    public IReadOnlyList<ValidationError> Validate(MkdsMapData obj)
    {
        var errors = new List<ValidationError>();

        var startPoints = obj.StartPoints;

        if (obj.IsMgStage && startPoints.Count < MG_START_POINT_COUNT)
        {
            errors.Add(new MkdsStartPointCountValidationError(this, ErrorLevel.Error, startPoints.FirstOrDefault(),
                $"Battle stages need {MG_START_POINT_COUNT} start points"));
        }
        
        if (!obj.IsMgStage && startPoints.Count > 1)
        {
            errors.Add(new MkdsStartPointCountValidationError(this, ErrorLevel.Error, startPoints[0],
                "Courses need 1 start point"));
        }

        if (!obj.IsMgStage)
            return errors;

        var usedIndices = new HashSet<int>();
        foreach (var startPoint in startPoints)
        {
            if (startPoint.Index is < 0 or >= MG_START_POINT_COUNT)
                errors.Add(new MkdsStartPointIndexValidationError(this, startPoint,
                    $"Start point index {startPoint.Index} is invalid"));
            else if (!usedIndices.Add(startPoint.Index))
                errors.Add(new MkdsStartPointIndexValidationError(this, startPoint,
                    $"Duplicate start point index {startPoint.Index}"));
        }

        var missingIndices = Enumerable.Range(0, MG_START_POINT_COUNT).Where(x => !usedIndices.Contains(x)).ToList();
        if (missingIndices.Count > 0 && startPoints.Count >= MG_START_POINT_COUNT)
            errors.Add(new MkdsStartPointMissingIndexValidationError(this, obj, missingIndices));

        return errors;
    }
}
