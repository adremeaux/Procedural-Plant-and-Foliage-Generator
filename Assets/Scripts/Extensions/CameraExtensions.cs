using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public static class CameraExtensions {
    public static void MatchCamera(this Camera mainCamera, Camera targetCam) {
      mainCamera.transform.position = targetCam.transform.position;
      mainCamera.transform.rotation = targetCam.transform.rotation;
      // mainCamera.transform.localScale = targetCam.transform.localScale;
      mainCamera.fieldOfView = targetCam.fieldOfView;
      mainCamera.nearClipPlane = targetCam.nearClipPlane;
      mainCamera.farClipPlane = targetCam.farClipPlane;
    }
  }
}
