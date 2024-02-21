using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using static BionicWombat.CoalescingTimer;
using static BionicWombat.Plant;

namespace BionicWombat {
  [Serializable]
  public struct LeafFactoryData {
    [HideInInspector] public LeafShape leafShape;
    [HideInInspector][NonSerialized] public LeafVeins leafVeins;
    [HideInInspector][NonSerialized] public DistortionCurve[][] distortionCurves;
    [HideInInspector][NonSerialized] public Mesh baseMesh;
    [HideInInspector][NonSerialized] public Mesh leafMesh;
    [HideInInspector] public Vector2 center;
    [HideInInspector] public Vector2 min;
    [HideInInspector] public Vector2 max;

    public Vector2 NormalizePoint(Vector2 point) =>
      new Vector2((point.x - min.x) / (max.x - min.x),
                  (point.y - min.y) / (max.y - min.y));
  }

  [Serializable]
  public class LeafFactory {

    public LeafFactoryData lfd;
    public LeafRenderer leafRenderer;

    public ComputeBuffer deltaVertsBuffer { get; private set; }
    public ComputeBuffer adjustedNormalsBuffer { get; private set; }
    public NormalVector[] surfaceNormals { get; private set; }
    public ComputeBuffer ageSpotsBuffer { get; private set; }

    private int deltaVertsLen = -1;

    public LeafFactory(LeafDeps baseDeps) {
      lfd = new LeafFactoryData();
      leafRenderer = new LeafRenderer();
    }

    private void ResetCurves() {
      lfd.leafShape = new LeafShape();
      lfd.leafVeins = new LeafVeins();
    }

    public static int DensityForRenderQuality(RenderQuality rq, int customDensity) {
      switch (rq) {
        case RenderQuality.Minimum: return 8;
        case RenderQuality.Medium:
        case RenderQuality.MediumThenMaximum: return 14;
        case RenderQuality.Maximum: return 48;
        case RenderQuality.Custom: return customDensity;
      }
      return 30;
    }

    public async Task<bool> RenderAll(LeafParamDict fields, Dictionary<LPType, bool> dirtyDict, RenderQuality renderQuality, LeafDeps deps, bool skipRender) {
      deps = deps.Copy();
      if (dirtyDict[LPType.Leaf] || dirtyDict[LPType.Vein]) {
        ResetCurves();

        lfd.leafShape.Render(fields, lfd.leafVeins, deps); //leafVeins is rendered within leafShape.Render()

        Vector2[] polyPoints = LeafRenderer.GetPolyPathPoints(
          LeafCurve.ToCurves(lfd.leafShape.curves, deps.leafData.GetTransform()),
          deps.baseParams.RenderLineSteps);
        (Vector2 min, Vector2 max) = LeafRenderer.GetBoundingBox(polyPoints);
        Vector2 center = LeafRenderer.GetNormalizedCenter(min, max);
        // DebugBW.Log("min: " + min + " | max: " + max + " | center: " + center);
        lfd.center = center;
        lfd.min = min;
        lfd.max = max;
      }

      int targetInstances = deps.baseParams.DistortionInstances;
      BWRandom.SetSeed(BWRandomPlantShop.GenTypedSeed(deps.leafData.randomSeed, LPType.Distort, 0));
      LeafDistortion distortion = new LeafDistortion(fields, lfd.leafShape.curves, lfd.leafVeins, deps);
      SplitTimer t = new SplitTimer("LeafFactory").Start();
      t.enable = false;
      lfd.distortionCurves = distortion.GetDistortionSplines(BWRandom.ManyFloats(targetInstances * 2), targetInstances);
      t.Split("GetDistortionSplines");

      if (!skipRender) {
        //create mesh
        if (dirtyDict[LPType.Leaf]) {
          List<Curve> rendCurves = LeafCurve.ToCurves(lfd.leafShape.curves);
          List<Curve> addCurves = new List<Curve>();
          if (deps.baseParams.TriangulateWithInnerVerts) addCurves.Add(lfd.leafVeins.GetMidrib());
          int lineSteps = deps.baseParams.RenderLineSteps;
          bool noRetry = deps.baseParams.RandomBS == "504";
          int density = DensityForRenderQuality(renderQuality, deps.baseParams.CustomMeshDensity);
          t.Split("Before Render");
          lfd.baseMesh = await leafRenderer.Render(rendCurves, addCurves, lineSteps, density,
            renderQuality == RenderQuality.Maximum && !noRetry, deps.baseParams.SubdivSteps, deps.baseParams.RandomBS);
          t.Split("After Render");

          LeafAgeGPU ageGPU = new LeafAgeGPU(lfd, deps);
          ageSpotsBuffer = ageGPU.ageSpots;
        }

        if (lfd.baseMesh == null) {
          //Debug.LogError("LeafFactory RenderAll dirty dict didn't render mesh: " + dirtyDict.ToLog());
          return false;
        } else {
          t.Split("Complete");
          return true;
        }
      }
      t.Split("Complete (Skipped)");
      return true;
    }

    public async Task<bool> DistortAndExtrudeLeafMeshes(ComputeShader distortCS, ComputeShader extrudeCS, ComputeShader normalsCS, LeafParamDict fields, int leafCount, Dictionary<LPType, bool> dirtyDict, LeafDeps deps, bool force = false) {
      if (!GlobalVars.instance.UseOldDistortion && deps.baseParams.RandomBS != "old") {
        return DistortAndExtrudeLeafMeshesGPU(distortCS, extrudeCS, normalsCS, fields, leafCount, dirtyDict, deps, force);
      } else {
        if (deltaVertsBuffer != null) {
          deltaVertsBuffer.Release();
          deltaVertsBuffer = null;
        }
        if (adjustedNormalsBuffer != null) {
          adjustedNormalsBuffer.Release();
          adjustedNormalsBuffer = null;
        }
        return await DistortAndExtrudeLeafMeshesOld(fields, leafCount, dirtyDict, deps, force);
      }
    }

    public bool DistortAndExtrudeLeafMeshesGPU(ComputeShader distortCS, ComputeShader extrudeCS, ComputeShader normalsCS,
        LeafParamDict fields, int leafCount, Dictionary<LPType, bool> dirtyDict, LeafDeps deps, bool force = false) {
      if (force || dirtyDict[LPType.Leaf] || dirtyDict[LPType.Vein] || dirtyDict[LPType.Distort]) {
        SplitTimer ts = new SplitTimer("DistortAndExtrudeGPU").Start();
        ts.enable = false;
        Mesh mesh = lfd.baseMesh.Copy();
        MeshData newVertsData = new MeshData();
        MeshData extrudeData = new MeshData();

        if (fields[LPK.ExtrudeEnabled].enabled) {
          Coalesce(ts, "Extrude Start");
          extrudeData.vertices = lfd.baseMesh.vertices;
          extrudeData.orderedEdgeVerts = EdgeFinder.FindEdgeVerts(mesh.triangles);
          extrudeData.triangles = mesh.triangles;
          extrudeData.uv = mesh.uv;
          extrudeData.colors = new Vector4[extrudeData.vertices.Length];
          Array.Fill<Vector4>(extrudeData.colors, Vector4.zero);
          Coalesce(ts, "Extrude Mid");

          LeafExtrudeGPU extrudeGPU = new LeafExtrudeGPU();
          newVertsData = extrudeGPU.ExtrudeMesh(extrudeCS,
            extrudeData,
            fields[LPK.ExtrudeEdgeDepth].value,
            fields[LPK.ExtrudeSuccThicc].value);

          Coalesce(ts, "Extrude Mesh");
          LeafRenderer.AssignToMesh(mesh, newVertsData);
          Coalesce(ts, "Assign To Mesh");
          lfd.leafMesh = mesh;

          extrudeGPU.Dispose();
        }

        LeafDistortGPU leafDistortGPU = new LeafDistortGPU(lfd.baseMesh.vertices);
        if (deltaVertsBuffer != null) deltaVertsBuffer.Release();

        // BWRandom.enabled = false;

        if (fields[LPK.DistortionEnabled].enabled && !deps.baseParams.HideDistortion) {
          for (int i = 0; i < leafCount; i++) {
            BWRandom.SetSeed(BWRandomPlantShop.GenTypedSeed(deps.leafData.randomSeed, LPType.Distort, i));
          }
          leafDistortGPU.Distort(distortCS,
            lfd.distortionCurves,
            lfd.leafVeins.GetMidrib(),
            fields,
            !deps.baseParams.HideDistortion,
            leafRenderer.leafWidth);
          deltaVertsBuffer = leafDistortGPU.deltaVertsRW;
          deltaVertsLen = leafDistortGPU.deltaVertsLen;
          Coalesce(ts, "Distort");

          if (fields[LPK.ExtrudeEnabled].enabled) {
            LeafNormalsGPU normalsGPU = new LeafNormalsGPU();
            normalsGPU.CalculateNormals(normalsCS, extrudeData, deltaVertsBuffer, deps.baseParams.DistortionInstances);
            if (adjustedNormalsBuffer != null) adjustedNormalsBuffer.Release();
            adjustedNormalsBuffer = normalsGPU.vertexNormalsBufferRW;

#if UNITY_EDITOR
            if (deps.inspector.showSurfaceNormals) {
              //Debug.LogWarning("ShowSurfaceNormals is on and slowing down rendering.");
              int instanceIdx = deps.baseParams.InstanceIdxFromRandomBS();
              surfaceNormals = normalsGPU.GetSurfaceNormals(instanceIdx);
            }
#endif

            normalsGPU.Dispose();
            normalsGPU = null;
            Coalesce(ts, "Normals");
          } else {
            Debug.LogWarning("Add code for Normals Calculation when extrude is disabled");
          }
        }
        leafDistortGPU.Dispose();

        // BWRandom.enabled = true;
        return true;
      }
      return false;
    }

    public async Task<bool> DistortAndExtrudeLeafMeshesOld(LeafParamDict fields, int leafCount, Dictionary<LPType, bool> dirtyDict, LeafDeps deps, bool force = false) {
      if (force || dirtyDict[LPType.Leaf] || dirtyDict[LPType.Vein] || dirtyDict[LPType.Distort]) {
        SplitTimer ts = new SplitTimer("DistortAndExtrudeCPU").Start();
        Mesh mesh = lfd.baseMesh.Copy();
        MeshData newVertsData = new MeshData();
        newVertsData.vertices = lfd.baseMesh.vertices;

        BWRandom.enabled = false;

        if (fields[LPK.DistortionEnabled].enabled) {
          object bwRandomLock = new object();
          for (int i = 0; i < leafCount; i++) {
            BWRandom.SetSeed(BWRandomPlantShop.GenTypedSeed(deps.leafData.randomSeed, LPType.Distort, i));
            newVertsData.randomNumbers = BWRandom.ManyFloats(deps.baseParams.DistortionInstances);
          }
          await Task.Run(() => {
            //distort
            Vector3[] newVerts = leafRenderer.Distort(lfd.distortionCurves[0].ToList(),
              newVertsData.vertices,
                lfd.leafVeins.GetMidrib(),
                fields,
                !deps.baseParams.HideDistortion);
            newVertsData.vertices = newVerts;
          });
          Coalesce(ts, "Distort");
          // try { for (int i = 0; i < leafCount; i++) LeafRenderer.AssignToMesh(meshes[i], newVertsData[i]); } catch (Exception e) { Debug.Log(e); }
          // Debug.Log("split 4: " + DateTime.Now.Subtract(runningTime).TotalMilliseconds); runningTime = DateTime.Now;
        }
        BWRandom.enabled = true;

        if (fields[LPK.ExtrudeEnabled].enabled) {
          Coalesce(ts, "Extrude Start");
          MeshData extrudeData;

          MeshData d = new MeshData();
          d.vertices = newVertsData.vertices;
          d.orderedEdgeVerts = EdgeFinder.FindEdgeVerts(mesh.triangles);
          d.triangles = mesh.triangles;
          d.uv = mesh.uv;
          extrudeData = d;
          Coalesce(ts, "Extrude Mid");

          await Task.Run(() => {
            //extrude mesh
            try {
              newVertsData = leafRenderer.ExtrudeMesh(extrudeData,
                fields[LPK.ExtrudeEdgeDepth].value,
                fields[LPK.ExtrudeSuccThicc].value);
            } catch (Exception e) {
              Debug.Log(e + "\n" + e.Source + "\n" + e.StackTrace);
            }
          });
          Coalesce(ts, "Task.Run");
          LeafRenderer.AssignToMesh(mesh, newVertsData);
          Coalesce(ts, "AssignToMesh");
        }
        lfd.leafMesh = mesh;
        return true;
      }
      return false;
    }

    public void InjectBufferData(Vector3[] deltaVerts, Vector3[] adjNormals) {
      if (deltaVertsBuffer != null) deltaVertsBuffer.Release();
      if (adjustedNormalsBuffer != null) adjustedNormalsBuffer.Release();

      deltaVertsBuffer = ComputeShaderBridge.MakeBuffer<Vector3>(deltaVerts.Length, deltaVerts);
      adjustedNormalsBuffer = ComputeShaderBridge.MakeBuffer<Vector3>(adjNormals.Length, adjNormals);
    }

    public Vector3[] ReadDeltaVertsBuffer() =>
      ComputeShaderBridge.ReadData<Vector3>(deltaVertsBuffer, deltaVertsLen);

    public Vector3[] ReadAdjNormalsBuffer() =>
     ComputeShaderBridge.ReadData<Vector3>(adjustedNormalsBuffer, deltaVertsLen);


    private void OnDestroy() {
      if (deltaVertsBuffer != null) deltaVertsBuffer.Release();
      if (adjustedNormalsBuffer != null) adjustedNormalsBuffer.Release();
    }
  }

}
