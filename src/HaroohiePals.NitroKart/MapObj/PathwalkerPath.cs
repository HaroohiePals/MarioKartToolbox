using HaroohiePals.NitroKart.MapData.Intermediate.Sections;

namespace HaroohiePals.NitroKart.MapObj;

sealed class PathwalkerPath
{
    public PathwalkerPathPart[] Parts;
    public bool Loop;

    public PathwalkerPath(MkdsPath path)
    {
        Loop = path.Loop;
        Parts = new PathwalkerPathPart[Loop ? path.Points.Count : path.Points.Count - 1];
        if (path.Points.Count == 2)
        {
            Parts[0] = new PathwalkerPathPart(false, true,
                null, path.Points[0].Position, path.Points[1].Position, null);
            if (Loop)
            {
                Parts[1] = new PathwalkerPathPart(false, true,
                    null, path.Points[1].Position, path.Points[0].Position, null);
            }
        }
        else
        {
            if (Loop)
            {
                Parts[0] = new PathwalkerPathPart(false, true,
                    path.Points[^1].Position, path.Points[0].Position,
                    path.Points[1].Position, path.Points[2].Position);
            }
            else
            {
                Parts[0] = new PathwalkerPathPart(false, true,
                    null, path.Points[0].Position, path.Points[1].Position,
                    path.Points[2].Position);
            }

            int v16;
            for (v16 = 1; v16 < path.Points.Count - 2; v16++)
            {
                Parts[v16] = new PathwalkerPathPart(false, true,
                    path.Points[v16 - 1].Position, path.Points[v16].Position,
                    path.Points[v16 + 1].Position, path.Points[v16 + 2].Position);
            }

            if (Loop)
            {
                Parts[v16] = new PathwalkerPathPart(false, true,
                    path.Points[v16 - 1].Position, path.Points[v16].Position,
                    path.Points[v16 + 1].Position, path.Points[0].Position);
            }
            else
            {
                Parts[v16] = new PathwalkerPathPart(false, true,
                    path.Points[v16 - 1].Position, path.Points[v16].Position,
                    path.Points[v16 + 1].Position, null);
            }

            if (Loop)
            {
                Parts[v16 + 1] = new PathwalkerPathPart(false, true,
                    path.Points[v16].Position, path.Points[v16 + 1].Position,
                    path.Points[0].Position, path.Points[1].Position);
            }
        }
    }
}