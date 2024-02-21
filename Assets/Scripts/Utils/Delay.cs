using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BionicWombat {
  public delegate void Action();
  public static class Delay : object {
    public static IEnumerator NextFrame(Action action) {
      yield return new WaitForEndOfFrame();
      action();
    }

    public static CancellationTokenSource After(int ms, Action action) {
      if (ms == 0) {
        action();
        return null;
      }

      CancellationTokenSource token = new CancellationTokenSource();
      _After(ms, action, token.Token);
      return token;
    }

    public static IEnumerator AfterCoroutine(int ms, Action action) {
      if (ms == 0) {
        action();
        yield break;
      }

      yield return new WaitForSeconds((float)ms / 1000f);
      action();
    }

#pragma warning disable CS0168 //unused var for TaskCanceledException
    private async static void _After(int ms, Action action, CancellationToken token) {
      try {
        await Task.Delay(ms, token).ConfigureAwait(false);
        action();
      } catch (TaskCanceledException e) {
        DebugBW.Log("    Delay cancelled");
      }
    }
#pragma warning restore CS0168
  }

}
