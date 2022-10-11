using System;
using UnityEngine;
using UImGui;
using ImGuiNET;

public class ImGui : MonoBehaviour
{
    private void Awake()
    {
        UImGui.UImGuiUtility.Layout += OnLayout;
    }

    private void OnDisable()
    {
        UImGuiUtility.Layout -= OnLayout;
    }

    private void OnLayout(UImGui.UImGui obj)
    {
        //ImGuiNET.ImGui.ShowDemoWindow();
    }

    public void AddJapaneseFont(ImGuiIOPtr io)
    {
        string fontPath = $"{Application.streamingAssetsPath}/Fonts/FirgeNerd-Regular.ttf";
        io.Fonts.AddFontFromFileTTF(fontPath, 18, null, io.Fonts.GetGlyphRangesJapanese());
    }

}
