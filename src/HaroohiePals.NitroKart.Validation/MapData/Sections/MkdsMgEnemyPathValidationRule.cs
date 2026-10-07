using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

class MkdsMgEnemyPathValidationRule : MkdsMapDataEntryValidationRule<MkdsMgEnemyPath>
{
    public override string Name => "Battle Enemy Path";

    protected override IReadOnlyList<ValidationError> Validate(MkdsMapData mapData, MkdsMgEnemyPath entry)
    {
        var errors = new List<ValidationError>();

        if (entry.Points.Count == 1 && entry.Next.Count == 0 && entry.Previous.Count == 0)
            errors.Add(new MkdsMgEnemyPathDeadEndValidationError(this, entry));

        return errors;
    }
}
