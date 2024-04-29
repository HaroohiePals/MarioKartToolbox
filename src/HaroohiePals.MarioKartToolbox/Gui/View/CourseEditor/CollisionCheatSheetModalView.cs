using HaroohiePals.Gui;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.MarioKartToolbox.KCollision;
using ImGuiNET;
using System.Numerics;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

internal class CollisionCheatSheetModalView : ModalView
{
    public CollisionCheatSheetModalView() : base("Collision attribute cheat sheet", new Vector2(ImGuiEx.CalcUiScaledValue(1000), ImGuiEx.CalcUiScaledValue(800)))
    {
    }

    protected override void DrawContent()
    {
        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Text("Name your materials using the following rules:");
            ImGui.BulletText("Separated with underscore, non case-sensitive: Type_Variant_ColorId_Shadow");
            ImGui.BulletText("The Type must be put before the Variant name");
            ImGui.BulletText("The Variant name can be omitted");
            ImGui.BulletText("Both Type and Variant can be referred using an integer. Example 0_1");
            ImGui.BulletText("Color and Shadow can be put in any order or omitted");
            ImGui.BulletText("Light Colors: C, Col, Color + ID. Valid examples: Color2, C1, Col3");
            ImGui.BulletText("Shadow on map: S, Shd, Shadow");
            ImGui.Text("Examples: Road_Dirt_Shadow_Color3, Wall_Color1, OutOfBounds");
        }

        if (ImGui.CollapsingHeader("Types and Variants", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Text("The Type/Variant string parts will be searched inside this table below.\nSpaces and parenthesis will be ignored when matching a result.");

            if (ImGui.BeginTable("TypesTable", 9, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuter))
            {
                ImGui.TableSetupColumn("Type");
                ImGui.TableSetupColumn("Variant 0");
                ImGui.TableSetupColumn("Variant 1");
                ImGui.TableSetupColumn("Variant 2");
                ImGui.TableSetupColumn("Variant 3");
                ImGui.TableSetupColumn("Variant 4");
                ImGui.TableSetupColumn("Variant 5");
                ImGui.TableSetupColumn("Variant 6");
                ImGui.TableSetupColumn("Variant 7");
                ImGui.TableHeadersRow();

                foreach (var type in MkdsCollisionConsts.ColTypes)
                {
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(type.Type.ToString());

                    for (int variantId = 0; variantId < 8; variantId++)
                    {
                        ImGui.TableSetColumnIndex(variantId + 1);
                        string[] variants = MkdsCollisionConsts.GetVariants(type.Type);
                        if (variantId < variants.Length)
                            ImGui.Text(variants[variantId]);
                        else
                            ImGui.TextDisabled("N/A");
                    }
                }

                ImGui.EndTable();
            }
        }
    }
}
