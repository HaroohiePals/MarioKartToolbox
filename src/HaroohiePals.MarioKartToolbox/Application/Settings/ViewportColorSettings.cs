using System.Drawing;

namespace HaroohiePals.MarioKartToolbox.Application.Settings;

record struct ViewportColorSettings()
{
    public Color MapObjects = Color.Red;
    public Color Paths = Color.FromArgb(0, 0, 171);
    public Color StartPoints = Color.FromArgb(32, 32, 32);
    public Color RespawnPoints = Color.Orange;
    public Color KartPoint2D = Color.FromArgb(0, 230, 255);
    public Color CannonPoints = Color.FromArgb(255, 0, 128);
    public Color KartPointMission = Color.MediumPurple;
    public Color EnemyPaths = Color.FromArgb(0, 204, 0);
    public Color MgEnemyPaths = Color.FromArgb(0, 204, 0);
    public Color ItemPaths = Color.FromArgb(204, 153, 0);
    public Color CheckPoint = Color.Blue;
    public Color KeyCheckPoint = Color.FromArgb(135, 206, 235);
    public Color Areas = Color.CornflowerBlue;
    public Color Cameras = Color.BurlyWood;
}