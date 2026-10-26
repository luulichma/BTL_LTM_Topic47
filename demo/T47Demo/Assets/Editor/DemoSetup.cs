using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Tạo scene demo và build bản Windows; chạy được từ menu "T47" hoặc dòng lệnh (-executeMethod).</summary>
public static class DemoSetup
{
    const string ScenePath = "Assets/Scenes/Demo.unity";
    const string BuildPath = "Build/T47Demo.exe";

    [MenuItem("T47/Tạo lại scene demo")]
    public static void CreateScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cam = new GameObject("Main Camera") { tag = "MainCamera" }.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.09f, 0.10f, 0.12f);
        new GameObject("T47 Demo").AddComponent<DemoApp>();

        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Debug.Log($"T47: đã tạo {ScenePath}");
    }

    [MenuItem("T47/Build Windows")]
    public static void BuildWindows()
    {
        if (!File.Exists(ScenePath)) CreateScene();

        PlayerSettings.productName = "T47 Demo";
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth = 1600;
        PlayerSettings.defaultScreenHeight = 900;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = BuildPath,
            target = BuildTarget.StandaloneWindows64,
        });

        bool ok = report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
        Debug.Log($"T47: build {report.summary.result} → {BuildPath}");
        if (Application.isBatchMode) EditorApplication.Exit(ok ? 0 : 1);
    }
}
