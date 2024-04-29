using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.MapObj;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.Course;

public class MkdsRequiredMapObjFilesValidationRule : IValidationRule<IMkdsCourse>
{
    private readonly IMkdsMapObjDatabase _mapObjDatabase;
    public string Name => "Required MapObj File";

    public MkdsRequiredMapObjFilesValidationRule(IMkdsMapObjDatabase mapObjDatabase)
    {
        _mapObjDatabase = mapObjDatabase;
    }

    public IReadOnlyList<ValidationError> Validate(IMkdsCourse obj)
    {
        var errors = new List<ValidationError>();

        if (obj.MapData.MapObjects is null)
            return errors;

        foreach (var mobj in obj.MapData.MapObjects)
        {
            string[]? requiredFiles = _mapObjDatabase.GetById(mobj.ObjectId)?.RequiredFiles;

            if (requiredFiles is null) 
                continue;

            foreach (var requiredFile in requiredFiles)
            {
                if (!MObjUtil.ExistsMapObjFile(obj, requiredFile))
                    errors.Add(new MkdsRequiredMapObjFileValidationError(this, requiredFile, mobj));
            }
        }

        return errors;
    }
}
