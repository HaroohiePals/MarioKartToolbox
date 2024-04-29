using HaroohiePals.NitroKart.Course;
using HaroohiePals.Validation;

namespace HaroohiePals.NitroKart.Validation.Course;

public interface IMkdsCourseValidatorFactory
{
    IValidator<IMkdsCourse> CreateMkdsCourseValidator();
}
