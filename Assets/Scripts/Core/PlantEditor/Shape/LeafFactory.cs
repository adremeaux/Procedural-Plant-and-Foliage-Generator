using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace BionicWombat {
[Serializable]
public struct LeafFactoryData {
  [HideInInspector] public LeafShape leafShape;
  [HideInInspector] public LeafVeins leafVeins;
  [HideInInspector] public List<DistortionCurve> distortionCurves;
  [HideInInspector] public Mesh leafMesh;
}

[Serializable]
public class LeafFactory {

  public LeafFactoryData lfd;
  public LeafRenderer leafRenderer;

  public LeafFactory(LeafDeps baseDeps) {
    lfd = new LeafFactoryData();
    leafRenderer = new LeafRenderer();
  }

  private void ResetCurves() {
    lfd.leafShape = new LeafShape();
    lfd.leafVeins = new LeafVeins();
  }

  public async Task RenderAll(LeafParamDict fields, Dictionary<LPType, bool> dirtyDict, LeafDeps deps, bool skipRender) {
    deps = deps.Copy();
    if (dirtyDict[LPType.Leaf] || dirtyDict[LPType.Vein]) {
      ResetCurves();

      lfd.leafShape.Render(fields, lfd.leafVeins, deps); //leafVeins is rendered within leafShape.Render()
    }

    LeafDistortion distortion = new LeafDistortion(fields, lfd.leafShape.curves, lfd.leafVeins, deps);
    lfd.distortionCurves = distortion.GetDistortionSplines();

    if (dirtyDict[LPType.Leaf] || dirtyDict[LPType.Vein] || dirtyDict[LPType.Distort]) {
      if (!skipRender) {
        //create mesh
        if (dirtyDict[LPType.Leaf]) {
          List<Curve> rendCurves = LeafCurve.ToCurves(lfd.leafShape.curves);
          List<Curve> addCurves = new List<Curve>();
          if (deps.baseParams.TriangulateWithInnerVerts) addCurves.Add(lfd.leafVeins.GetMidrib());
          lfd.leafMesh = await leafRenderer.Render(rendCurves, addCurves, deps.baseParams.RenderLineSteps, deps.baseParams.SubdivSteps, deps.baseParams.RandomBS);
        }
        if (lfd.leafMesh == null) {
          Debug.LogError("LeafFactory RenderAll dirty dict didn't render mesh: " + dirtyDict.ToLog());
          return;
        }

        //distort
        if (fields[LPK.DistortionEnabled].enabled)
          leafRenderer.Distort(lfd.leafMesh,
            lfd.distortionCurves,
            lfd.leafVeins.GetMidrib(),
            fields,
            !deps.baseParams.HideDistortion);

        //extrude mesh
        if (fields[LPK.ExtrudeEnabled].enabled)
          leafRenderer.ExtrudeMesh(lfd.leafMesh,
            fields[LPK.ExtrudeEdgeDepth].value,
            fields[LPK.ExtrudeSuccThicc].value,
            !fields[LPK.DistortionEnabled].enabled);
      }
    }
  }
}

}