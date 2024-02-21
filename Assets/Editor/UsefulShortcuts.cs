using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BionicWombat {
  public static class UsefulShortcuts {

    public delegate void DidRefreshEditor();
    public static event DidRefreshEditor DidRefreshEditorEvent;

    [MenuItem("Window/Clear Console %#c")] // CTRL + SHIFT + C
    public static void ClearConsole() {
      var assembly = System.Reflection.Assembly.GetAssembly(typeof(SceneView));
      var type = assembly.GetType("UnityEditor.LogEntries");
      var method = type.GetMethod("Clear");
      method.Invoke(new object(), null);
    }

    [MenuItem("Window/Refresh and Clear %#r")] // CTRL + SHIFT + R
    public static void RefreshAndClear() {
      var assembly = System.Reflection.Assembly.GetAssembly(typeof(SceneView));
      var type = assembly.GetType("UnityEditor.LogEntries");
      var method = type.GetMethod("Clear");
      method.Invoke(new object(), null);
      AssetDatabase.Refresh();
      DidRefreshEditorEvent?.Invoke();
    }

    private static List<string> specificAssets = new List<string>();
    public static void RefreshSpecific(params string[] names) {
      specificAssets.Clear();
      specificAssets.AddRange(names);
      Debug.Log(System.Threading.Thread.CurrentThread.IsBackground);

      foreach (string search in names) {
        int pipeIdx = search.IndexOf("|");
        string nameSearch = "";
        string s = search;
        if (pipeIdx != -1) {
          s = search.Substring(0, pipeIdx);
          nameSearch = search.Substring(pipeIdx + 1, search.Length - pipeIdx - 1);
        }

        string[] guids = AssetDatabase.FindAssets(s);
        List<string> paths = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToList();
        if (nameSearch.HasLength()) {
          paths = paths.Filter(p => p.Contains(nameSearch)).ToList();
        }
        DebugBW.Log("paths: " + paths.ToLog());
        foreach (string path in paths) AssetDatabase.ImportAsset(path);
      }
    }

    public static string[] PostprocessAssetsToCheck() {
      return specificAssets.ToArray();
    }

    public static void DidPostprocessAsset(string asset) {
      // DebugBW.Log("asset: " + asset);
    }

    [MenuItem("Window/Reload Scene %#&r")] // CTRL + SHIFT + ALT + R
    public static void ReloadScene() {
      Scene scene = SceneManager.GetActiveScene();
      EditorSceneManager.OpenScene(scene.path);
    }

    private static bool isSet = false;
    private static bool shouldRunAfterCompile = false;
    private static bool isCompiling = false;
    [MenuItem("Window/Compile and Run %r")] // CTRL + R
    public async static void CompileAndRun() {
      ClearConsole();
      Debug.Log("Compile and Run");
      if (!isSet) {
        CompilationPipeline.compilationFinished += CompilationFinished;
        CompilationPipeline.compilationStarted += CompilationStarted;
        isSet = true;
      }
      shouldRunAfterCompile = true;
      EditorApplication.isPlaying = false; //stop a running instance to recompile
      AssetDatabase.Refresh();
      await Task.Delay(500);
      if (!isCompiling) CompilationFinished();

      //UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
    }

    private static DateTime timeDelta;
    private static void CompilationStarted(object ctx) {
      timeDelta = DateTime.Now;
      Debug.Log("Start Compile [" + timeDelta.ToString("HH:mm:ss.fff") + "]");
      isCompiling = true;
    }

    private static void CompilationFinished(object ctx = null) {
      if (shouldRunAfterCompile) {
        EditorApplication.isPlaying = true;
        double ms = DateTime.Now.Subtract(timeDelta).TotalMilliseconds;
        if (ctx != null)
          Debug.Log("Compilation Complete in " + ms + "ms");
        else
          Debug.Log("Compilation Skipped");
      }
      shouldRunAfterCompile = false;
      isCompiling = false;
    }

    [MenuItem("Tools/Toggle Inspector Lock %l")] // Ctrl + L
    static void ToggleInspectorLock() {
      ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
      ActiveEditorTracker.sharedTracker.ForceRebuild();
    }

    /*private static void RecompileUnityEditor() {
        BuildTargetGroup target = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        string rawSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);

        PlayerSettings.SetScriptingDefineSymbolsForGroup(target, rawSymbols + "a");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(target, rawSymbols);
    }*/
  }

  public class ShortcutsImportPostProcesser : AssetPostprocessor {
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
      string[] assets = UsefulShortcuts.PostprocessAssetsToCheck();
      // DebugBW.Log("ASDASDassets: " + assets.ToLog());
      foreach (string ass in assets)
        if (assets.Contains(ass))
          UsefulShortcuts.DidPostprocessAsset(ass);
    }
  }

  // public class PostProcessImportAsset : AssetPostprocessor {
  //   //Based on this example, the output from this function should be:
  //   //  OnPostprocessAllAssets
  //   //  Imported: Assets/Artifacts/test_file01.txt
  //   //
  //   //test_file02.txt should not even show up on the Project Browser
  //   //until a refresh happens.
  //   static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
  //     Debug.Log("OnPostprocessAllAssets");

  //     foreach (var imported in importedAssets)
  //       Debug.Log("Imported: " + imported);

  //     foreach (var deleted in deletedAssets)
  //       Debug.Log("Deleted: " + deleted);

  //     foreach (var moved in movedAssets)
  //       Debug.Log("Moved: " + moved);

  //     foreach (var movedFromAsset in movedFromAssetPaths)
  //       Debug.Log("Moved from Asset: " + movedFromAsset);
  //   }
  // }
}
