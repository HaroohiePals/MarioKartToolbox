using System;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

enum MapDataGeneratorSourceType
{
    EnemyPath,
    ItemPath
}

[Flags]
enum MapDataGeneratorFlags
{
    GenerateEnemyPath = 1,
    GenerateItemPath = 2,
    GenerateCheckPoint = 4,
    GenerateRespawn = 8,

    UpdateRespawnReferences = 16,
    UpdateCheckPointReferences = 32
}

struct MapDataGeneratorSettings
{
    public MapDataGeneratorSourceType SourceType;
    public MapDataGeneratorFlags Flags;

    public int RespawnPointSkip;
    public bool RespawnPointKeepPathBoundaries;

    public double CheckPointWidth;
    public int CheckPointPointSkip;
    public bool CheckPointKeepPathBoundaries;
}