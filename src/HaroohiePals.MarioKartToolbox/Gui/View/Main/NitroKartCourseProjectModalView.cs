using HaroohiePals.Gui.View.Modal;
using ImGuiNET;
using NativeFileDialogs.Net;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace HaroohiePals.MarioKartToolbox.Gui.View.Main
{
    internal class NitroKartCourseProjectModalView : ModalView
    {
        private string _projectPath = "";
        private string _romPath = "";
        private bool _isMgCourse = false;

        public Action<string> OnProjectCreated;

        public NitroKartCourseProjectModalView() : base("Create a New Course...", new Vector2(600, 150))
        {

        }

        protected override void DrawContent()
        {
            var region = ImGui.GetContentRegionAvail();

            if (ImGui.BeginChild("##ChildContent", new Vector2(region.X, region.Y - 30)))
            {
                DrawChildContent();
                ImGui.EndChild();
            }

            ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - 1 * 80 - ImGui.GetStyle().ItemSpacing.X);


            if (string.IsNullOrEmpty(_projectPath))
                ImGui.BeginDisabled();
            if (ImGui.Button("Create", new Vector2(80, 0)))
            {
                CreateProject();
            }
            if (string.IsNullOrEmpty(_projectPath))
                ImGui.EndDisabled();
        }

        private void DrawChildContent()
        {
            ImGui.Columns(2);

            ImGui.Text("Project File Destination Path");
            ImGui.NextColumn();
            string projectPath = _projectPath;
            ImGui.InputText("##Path", ref projectPath, 10000);
            ImGui.SameLine();
            if (ImGui.Button("Browse...##Path"))
            {
                SelectProjectFileSavePath();
            }
            ImGui.NextColumn();

            ImGui.Text("ROM Path (Optional)");
            ImGui.NextColumn();
            string romPath = _romPath;
            ImGui.InputText("##ROM Path (Optional)", ref romPath, 10000);
            ImGui.SameLine();
            if (ImGui.Button("Browse...##RomPath"))
            {
                SelectRomPath();
            }
            ImGui.NextColumn();

            ImGui.Text("Battle Course");
            ImGui.NextColumn();
            ImGui.Checkbox("##Battle Course", ref _isMgCourse);
            ImGui.NextColumn();

            ImGui.Columns(1);
        }

        private void SelectProjectFileSavePath()
        {
            var result = Nfd.SaveDialog(out string outPath, new Dictionary<string, string> { { "XML", "xml" } });

            if (result == NfdStatus.Ok)
            {
                _projectPath = outPath;
            }
        }

        private void SelectRomPath()
        {
            var result = Nfd.OpenDialog(out string outPath, new Dictionary<string, string>
            {
                { "Nintendo DS ROM File", "nds" },
                { "Nitro Kart Project", "nkproj" },
                { "XML", "xml" },
            });

            if (result == NfdStatus.Ok)
            {
                _romPath = outPath;
            }
        }

        private void CreateProject()
        {
            if (NitroKartCourseProject.Create(_projectPath, _romPath, _isMgCourse))
            {
                OnProjectCreated?.Invoke(_projectPath);
                Close();
            }
        }
    }
}
