using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BionicWombat {
#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  public class Dispatcher : MonoBehaviour {
    public static void RunAsync(Action action) {
      if (_instance == null) Initialize();
      ThreadPool.QueueUserWorkItem(o => action());
    }

    public static void RunAsync(LogAction<object> action, object state) {
      if (_instance == null) Initialize();
      ThreadPool.QueueUserWorkItem(o => action(o), state);
    }

    public static void RunOnMainThread(Action action) {
      if (_instance == null) Initialize();
      lock (_backlog) {
        _backlog.Add(action);
        _queued = true;
      }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize() {
      if (_instance == null) {
        _instance = new GameObject("Dispatcher").AddComponent<Dispatcher>();
        DontDestroyOnLoad(_instance.gameObject);
        Debug.Log("dispatcher init");
      }
    }

    private void Update() {
      if (_queued) {
        lock (_backlog) {
          var tmp = _actions;
          _actions = _backlog;
          _backlog = tmp;
          _queued = false;
        }

        foreach (var action in _actions)
          action();

        _actions.Clear();
      }
    }

    static Dispatcher _instance;
    static volatile bool _queued = false;
    static List<Action> _backlog = new List<Action>(8);
    static List<Action> _actions = new List<Action>(8);
  }
}
