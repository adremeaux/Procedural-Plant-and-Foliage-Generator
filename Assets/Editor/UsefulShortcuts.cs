using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BionicWombat {
public static class UsefulShortcuts {
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

  /*private static void RecompileUnityEditor() {
      BuildTargetGroup target = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
      string rawSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);

      PlayerSettings.SetScriptingDefineSymbolsForGroup(target, rawSymbols + "a");
      PlayerSettings.SetScriptingDefineSymbolsForGroup(target, rawSymbols);
  }*/
}
}