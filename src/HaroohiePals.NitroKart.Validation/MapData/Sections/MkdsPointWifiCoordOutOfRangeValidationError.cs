using HaroohiePals.MarioKart.MapData;
using HaroohiePals.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaroohiePals.NitroKart.Validation.MapData.Sections;

internal class MkdsPointWifiCoordOutOfRangeValidationError : ValidationError
{
    public MkdsPointWifiCoordOutOfRangeValidationError(IValidationRule rule, IPoint source) 
        : base(rule, ErrorLevel.Warning, "Point out of range for a Wi-Fi race (-4096, 4096)", source, false)
    {
    }
}
