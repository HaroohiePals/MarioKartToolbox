using HaroohiePals.NitroKart.MapData;
using System.Collections.Generic;

namespace HaroohiePals.NitroKart.MapObj;

public interface IMkdsMapObjDatabase
{
    IReadOnlyList<MkdsMapObjInfo> GetAll();
    MkdsMapObjInfo GetById(MkdsMapObjectId id);
}
