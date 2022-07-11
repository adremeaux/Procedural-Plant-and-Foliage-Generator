using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BionicWombat {
public delegate void Action();
public static class Delay : object {
  public static CancellationTokenSource After(int ms, Action action) {
    if (ms == 0) {
      action();
      return null;
    }

    CancellationTokenSource token = new CancellationTokenSource();
    _After(ms, action, token.Token);
    return token;
  }

#pragma warning disable CS0168 //unused var for TaskCanceledException
  private async static void _After(int ms, Action action, CancellationToken token) {
    try {
      await Task.Delay(ms, token).ConfigureAwait(false);
    } catch (TaskCanceledException e) {
    }
    action();
  }
#pragma warning restore CS0168
}

}