using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BionicWombat {
  public interface ISceneClickResponder {
    void DidClickChild(GameObject child);
  }

  public class SceneClickMonitor : MonoBehaviour {
    public Weak<ISceneClickResponder> responder;
    public bool active = true;

    public void SetResponder(ISceneClickResponder responder) {
      this.responder = new Weak<ISceneClickResponder>(responder);
    }

    private bool hasWarned = false;
    private void Update() {
      if (active) {
        if (!hasWarned && !Asserts.AssertWarning(responder.Check(), "SceneClickMonitor responder must be set: " + this)) {
          hasWarned = true;
          return;
        }
        RaycastHit hit;
        Camera camera = Camera.allCameras[0];
        if (camera != null && Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit)) {
          GameObject go = hit.transform.gameObject;
          // DebugBW.Log("SceneClickMonitor clicked: " + go + " (Check props container)");
          if (go.transform.IsChildOf(this.transform)) {
            if (Input.GetMouseButtonDown(0)) {
              responder.obj.DidClickChild(go);
            }
          }
        }
      }
    }
  }
}
