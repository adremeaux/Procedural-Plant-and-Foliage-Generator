using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImageMagick;
using UnityEngine;
using static BionicWombat.ColorExtensions;

namespace BionicWombat {
  public class IMTextureVars {
    public int offset = 0;
    public int imgSize = 1024;
    public int downsample = 1; //higher = lower qual
    public int normalSupersample = 1;
    public int shadowSize = 64;
    public int imgSizeScaled => imgSize / downsample;
    public int shadowSizeScaled => shadowSize / downsample;
    public int shadowSizeScaledLess => 256 / downsample;
    public string randomBS;

    public int lineSteps;
    public int veinLineSteps;

    public Color shadowColor;
    public Color radianceColor;

    public List<LeafCurve> curves;
    public LeafVeins veins;
    public LeafParamDict fields;
    public List<IMRenderableVein> rendVeins;
    public PointD[] leafPoints;
    public List<PointD[]> puffies;
    public PlantIndexEntry indexEntry;
    public PlantCollection collection;

    public float VAL(LPK key) => fields[key].value;
    public Color COLOR(LPK key) => fields[key].colorValue;
    public bool ENABLED(LPK key) => fields[key].enabled;
  }

  [Serializable]
  public class TextureCommandsModel {
    public List<IMTextureCommand> commands;
    public List<PixelFillCommand> pfcs = new List<PixelFillCommand>();
    public bool useMask;

    public Dictionary<string, IMTextureCommand> cachedCmdsDict;
    public TextureCommandDict cachedCmdsDictBools;
    public bool cachedCmdsDictDirty = false;
  }

  public enum TextureType {
    Albedo,
    Normal,
    Height,
    VeinMask,
    Clipping,
  }

  [Serializable]
  public class IMTextureFactory {
    private MagickImage image;
    private MagickImage mask;

    private TextureCommandsModel model;
    private IMTextureVars vars;

    private DateTime runningTime;
    private string currentMethod;
    private LeafDeps deps;

    public bool enabled = true;

    public IMTextureFactory(TextureType type) {
      // Debug.Log("IMTextureFactory init " + type);
      model = new TextureCommandsModel();
      if (type == TextureType.Albedo) {
        model.commands = new List<IMTextureCommand>() {
        new IMTextureCmdDrawGradient(),
        new IMTextureCmdDrawCellsOverlay(),
        new IMTextureCmdDrawInnerShadow(60, 5, CompositeOperator.Multiply),
        new IMTextureCmdDrawBaseShadow(),
        new IMTextureCmdDrawInnerShadow(20, 1.5, CompositeOperator.Overlay),
        new IMTextureCmdDrawVeinBackdrop(),
        new IMTextureCmdDrawPrimaryVeinsRadiance(),
        new IMTextureCmdDrawPrimaryVeinsMain(),
        new IMTextureCmdDrawMargin(),
        new IMTextureCmdDrawHairlineVeins(),
      };
        model.useMask = true;
      } else if (type == TextureType.Normal) {
        model.commands = new List<IMTextureCommand>() {
        new IMTextureCmdDrawColor(NormalColors.Facing),
        new IMTextureCmdDrawPuffyNormals(),
        new IMTextureCmdDrawPrimaryVeinsNormals(),
        new IMTextureCmdDrawTestNormals(),
      };
        model.useMask = false;
      } else if (type == TextureType.Height) {
        model.commands = new List<IMTextureCommand>() {
        new IMTextureCmdDrawColor(UnityEngine.Color.black),
        new IMTextureCmdDrawPuffyNormals(true),
        new IMTextureCmdDrawPrimaryVeinsNormals(true),
      };
      } else if (type == TextureType.VeinMask) {
        model.commands = new List<IMTextureCommand>() {
        new IMTextureCmdDrawColor(UnityEngine.Color.black),
        new IMTextureCmdDrawPrimaryVeinsMain(UnityEngine.Color.white, false),
      };
      } else if (type == TextureType.Clipping) {
        model.commands = new List<IMTextureCommand>();
      } else {
        Debug.LogError("IMTextureFactory type does not define draw commands: " + type);
      }
    }

    public TextureCommandDict GetTextureCommandStrings(TextureType type) {
      if (model == null) return null;

      if (model.cachedCmdsDictDirty == true ||
          model.cachedCmdsDict == null ||
          model.cachedCmdsDict.Count == 0) {
        model.cachedCmdsDict = new Dictionary<string, IMTextureCommand>();
        model.cachedCmdsDictBools = new TextureCommandDict();
        foreach (IMTextureCommand command in model.commands) {
          string name = command.GetType().Name;
          while (model.cachedCmdsDict.ContainsKey(name)) name += "2";
          model.cachedCmdsDict.Add(name, command);
          model.cachedCmdsDictBools.Add(name, command.enabled);
        }
      }
      return model.cachedCmdsDictBools;
    }

    public void SetTextureCommandStrings(TextureCommandDict dict, TextureType type) {
      if (model.cachedCmdsDict == null || model.cachedCmdsDict.Count == 0) GetTextureCommandStrings(type);

      foreach (string name in dict.Keys) {
        if (model.cachedCmdsDict.ContainsKey(name)) {
          IMTextureCommand c = model.cachedCmdsDict[name];
          c.enabled = dict[name];
        }
      }
      model.cachedCmdsDictDirty = true;

      if (dict.ContainsKey("enabled")) enabled = dict["enabled"];
    }

    public static IMTextureVars GetTextureVars(
        List<LeafCurve> curves, LeafVeins veins, LeafParamDict fields, LeafDeps deps,
        PlantIndexEntry entry, PlantCollection collection) {
      BaseParams baseParams = deps.baseParams;
      IMTextureVars vars = new IMTextureVars();
      vars.imgSize = baseParams.TextureSize;
      vars.normalSupersample = baseParams.NormalSupersample;
      vars.curves = curves;
      vars.fields = fields;
      vars.lineSteps = baseParams.RenderLineSteps;
      vars.downsample = baseParams.TextureDownsample;
      vars.veinLineSteps = baseParams.VeinLineSteps;
      vars.veins = veins;
      vars.indexEntry = entry;
      vars.collection = collection;
      vars.randomBS = deps.baseParams.RandomBS;

      (vars.leafPoints, vars.rendVeins, vars.puffies) = GetScaledPoints(curves, veins, fields, vars.lineSteps, vars.veinLineSteps, vars.imgSizeScaled);

      vars.shadowColor = LeafParamBehaviors.GetColorForParam(fields[LPK.TexShadowStrength], fields);
      vars.radianceColor = LeafParamBehaviors.GetColorForParam(fields[LPK.TexRadianceHue], fields);

      return vars;
    }

    public void Prepare(LeafDeps deps, IMTextureVars textureVars) {
      this.deps = deps;
      vars = textureVars;

      // OpenCL.IsEnabled = true;
    }

    private bool _startLoggin = false;

    public async Task DrawTexture(TextureType type, int downsampleOverride = -1) {
      if (!enabled) return;

      bool didFinish = false;
      if (image != null) {
        Debug.LogWarning("DrawTexture image hasn't been disposed. Aborting render.");
        return;
      }

      void L(int n) { if (_startLoggin) Debug.Log(type + ": " + n + " (" + type + ")"); }

      try {
        DateTime timeDelta = DateTime.Now;
        if (downsampleOverride != -1 && downsampleOverride != 1) {
          vars.downsample = downsampleOverride;
          (vars.leafPoints, vars.rendVeins, vars.puffies) =
            GetScaledPoints(vars.curves, vars.veins, vars.fields, vars.lineSteps, vars.veinLineSteps, vars.imgSizeScaled);
        }

        image = new MagickImage(MagickColors.Black, vars.imgSizeScaled, vars.imgSizeScaled);
        if (model.useMask) CreateMask();

        L(1);
        foreach (IMTextureCommand cmd in model.commands) {
          cmd.SetVars(vars, deps.logOptions.logTextureSplits);
        }

        await Task.Run(() =>
          Parallel.ForEach<IMTextureCommand>(model.commands, cmd => {
            if (cmd.enabled) cmd.Prepare();
          })
        );

        L(2);
        foreach (IMTextureCommand cmd in model.commands)
          if (cmd.enabled) cmd.Composite(image);

        L(3);
        model.pfcs = new List<PixelFillCommand>();
        foreach (IMTextureCommand cmd in model.commands)
          if (cmd.enabled && (cmd is IMPixelCommand pc))
            model.pfcs.AddRange(pc.GetPixelFillCommands());

        L(4);
        if (model.useMask) {
          image.Composite(mask, CompositeOperator.CopyAlpha);
          image.Alpha(AlphaOption.Remove);
        }

        L(5);
        PlantDataManager.WriteIMTexture(image, vars.indexEntry, type, vars.collection);

#if UNITY_EDITOR
        if (!Application.isPlaying)
          UnityEditor.AssetDatabase.Refresh();
#endif

        L(6);
        ApplyPixelCommands(type, vars.collection);

        double ms = DateTime.Now.Subtract(timeDelta).TotalMilliseconds;
        if (deps.logOptions.logTextureSplits)
          Debug.Log(type.ToString() + " texture generated in " + ms + "ms" + " downsampled " + (1f / (float)vars.downsample));
        didFinish = true;
      } catch (Exception e) {
        Debug.LogError("IMTextureFactory exception: " + e);
      } finally {
        if (!didFinish) {
          Debug.LogWarning("DrawTexture Finally clause before finishing");
          _startLoggin = true;
        } else {
          _startLoggin = false;
        }
        if (image != null) image.Dispose();
        if (mask != null) mask.Dispose();
        image = null;
        mask = null;
      }
    }

    private void ApplyPixelCommands(TextureType type, PlantCollection collection) {
      Texture2D rgb = PlantDataManager.GetTexture(vars.indexEntry, type,
        "pxcmd " + vars.indexEntry.SafeName(), collection);
      if (!rgb.isReadable) rgb = GetReadableTexture(rgb);

      if (model.pfcs.Count > 0) {
        foreach (PixelFillCommand pfc in model.pfcs) {
          RectInt croppedRect = CropRect(pfc.rect, rgb.width, rgb.height);
          if (croppedRect.width == 0 || croppedRect.height == 0) {
            //Debug.LogWarning("PixelCommand has negative dimensions: " + pfc + " | cropped: " + croppedRect);
            continue;
          }
          pfc.CropToRect(croppedRect);
          RectInt mirroredRect = MirrorRect(croppedRect, vars.imgSizeScaled);
          // Debug.Log("w/h: " + rgb.width + ", " + rgb.height + " | pfc.rect: " + pfc.rect + " | crop: " + croppedRect + " | mir: " + mirroredRect);
          Color[] pixels = rgb.GetPixels(mirroredRect.x, mirroredRect.y, mirroredRect.width, mirroredRect.height);
          PixelFillCommand tPfc = BlendPFCColors(pfc, pixels, vars.imgSizeScaled);
          rgb.SetPixels((int)tPfc.rect.x, (int)tPfc.rect.y, (int)tPfc.rect.width, (int)tPfc.rect.height, tPfc.colors);
        }
        rgb.Apply();
      }

      PlantDataManager.WriteTexture(rgb, vars.indexEntry, type, collection);
    }

    private static Texture2D ConvertToRGB24(Texture2D tex) {
      Texture2D rgb = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);
      rgb.SetPixels(tex.GetPixels());
      rgb.Apply();
      return rgb;
    }

    public static Texture2D GetReadableTexture(Texture2D source) {
      RenderTexture renderTex = RenderTexture.GetTemporary(
                  source.width,
                  source.height,
                  0,
                  RenderTextureFormat.Default,
                  RenderTextureReadWrite.Linear);

      Graphics.Blit(source, renderTex);
      RenderTexture previous = RenderTexture.active;
      RenderTexture.active = renderTex;
      Texture2D readableText = new Texture2D(source.width, source.height,
        UnityEngine.Experimental.Rendering.DefaultFormat.LDR, 1, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
      readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
      readableText.Apply();
      RenderTexture.active = previous;
      RenderTexture.ReleaseTemporary(renderTex);
      return readableText;
    }

    private static PixelFillCommand BlendPFCColors(PixelFillCommand pfc, Color[] basePixels, float imgSizeScaled) {
      Color[] tColors = new Color[pfc.colors.Length];
      int w = pfc.rect.width;
      int h = pfc.rect.height;
      for (int y = 0; y < h; y++) {
        for (int x = 0; x < w; x++) {
          int newY = h - y - 1;
          Color nc = pfc.colors[y * w + x];
          if (pfc.useAlpha && nc.a < 1f) {
            Color baseColor = basePixels[newY * w + x];
            nc = ColorLerp(baseColor, nc, nc.a);
          } else if (nc.a < 1f) {
            nc = ColorLerp(UnityEngine.Color.black, nc, nc.a);
          }
          tColors[newY * w + x] = nc;
        }
      }
      PixelFillCommand nPfc = new PixelFillCommand();
      nPfc.colors = tColors;
      nPfc.rect = MirrorRect(pfc.rect, imgSizeScaled);
      nPfc.useAlpha = pfc.useAlpha;
      return nPfc;
    }

    private static RectInt MirrorRect(RectInt rect, float imgSizeScaled) => new RectInt(rect.x, (int)imgSizeScaled - rect.y - rect.height, rect.width, rect.height);

    private static RectInt CropRect(RectInt rect, int width, int height) {
      // Debug.Log(rect.height + " | y: " + rect.y + " | " + rect.yMin + " | " + rect.yMax);
      if (rect.width < 0 || rect.height < 0 || rect.x >= width || rect.y >= height) return new RectInt(0, 0, 0, 0);
      if (rect.xMax < width && rect.yMax < height && rect.xMin >= 0 && rect.yMin >= 0) return rect;

      if (rect.yMin < 0) {
        return new RectInt(rect.xMin, 0, rect.width, rect.height + rect.yMin);
      } else if (rect.yMax >= height) {
        if (rect.xMax >= width) {
          return new RectInt(rect.xMin, rect.yMin, width - rect.xMin, height - rect.yMin);
        }
        return new RectInt(rect.xMin, rect.yMin, rect.width, height - rect.yMin);
      } else if (rect.xMax >= width) {
        return new RectInt(rect.xMin, rect.yMin, width - rect.xMin, rect.height);
      }
      Debug.LogError("CropRect unreachable state");
      return rect;
    }

    private void CreateMask() {
      mask = new MagickImage(MagickColors.Black, vars.imgSizeScaled, vars.imgSizeScaled);
      mask.Draw(new DrawablePolygon(vars.leafPoints), new DrawableFillColor(MagickColors.White));
      mask.Alpha(AlphaOption.Off);
    }

    private static (PointD[], List<IMRenderableVein>, List<PointD[]>) GetScaledPoints(
      List<LeafCurve> curves, LeafVeins leafVeins, LeafParamDict fields,
      int lineSteps, int veinLineSteps, float imgSizeScaled
    ) {
      Vector2[] points = LeafRenderer.GetPolyPathPoints(LeafCurve.ToCurves(curves), lineSteps);
      (Vector2 min, Vector2 max) = LeafRenderer.GetBoundingBox(points);
      float spanX = max.x - min.x;
      float spanY = max.y - min.y;
      Vector2 offset = new Vector2(-min.x, -min.y);
      float scale = spanX > spanY ? imgSizeScaled / spanX : imgSizeScaled / spanY;
      if (spanY > spanX) offset = new Vector2(
        offset.x + (imgSizeScaled - (imgSizeScaled * (spanX / spanY))) / 2f / scale,
        offset.y);
      else offset = new Vector2(
        offset.x,
        offset.y + (imgSizeScaled - (imgSizeScaled * (spanY / spanX))) / 2f / scale);

      PointD CalcPointD(Vector2 p) {
        float pdX = (p.x + offset.x) * scale;
        float pdY = (p.y + offset.y) * scale;
        pdY = pdY * -1f + imgSizeScaled;
        return new PointD(pdX, pdY);
      };

      PointD[] pds = new PointD[points.Length];
      for (int i = 0; i < points.Length; i++) {
        pds[i] = CalcPointD(points[i]);
      }

      List<PointD[]> puffies = new List<PointD[]>();
      List<Vector3[]> puffVs = leafVeins.GetPuffyPolys();
      foreach (Vector3[] vecs in puffVs) {
        PointD[] tempArr = new PointD[vecs.Length];
        for (int i = 0; i < vecs.Length; i++) {
          tempArr[i] = CalcPointD(vecs[i]);
        }
        puffies.Add(tempArr);
        // Debug.Log("Calc: " + vecs.ToLogShort() + " => " + tempArr.ToLogShort());
      }

      PointD[] PointsWithWidthMod(LeafVein vein, float widthAdd, float widthMult) {
        List<Vector2> list = vein.AsPoly(widthAdd, widthMult);
        PointD[] arr = new PointD[list.Count];
        for (int i = 0; i < list.Count; i++)
          arr[i] = CalcPointD(list[i]);
        return arr;
      };

      List<IMRenderableVein> rendVeins = new List<IMRenderableVein>();
      foreach (LeafVein vein in leafVeins.GetVeins()) {
        Vector2[] veinVPoints = LeafRenderer.GetPolyPathPoints(vein, veinLineSteps);
        PointD[] veinPoints = new PointD[veinVPoints.Length];
        for (int i = 0; i < veinPoints.Length; i++)
          veinPoints[i] = CalcPointD(veinVPoints[i]);

        float normalWidthVal = LeafVein.IsPrimaryType(vein.type) ?
            fields[LPK.NormalMidribWidth].value : fields[LPK.NormalSecondaryWidth].value;

        IMRenderableVein rv = new IMRenderableVein(vein,
          PointsWithWidthMod(vein, 0f, 1f),
          PointsWithWidthMod(vein, 0f, fields[LPK.TexRadianceWidthMult].value),
          PointsWithWidthMod(vein, 0.2f, 1f),
          PointsWithWidthMod(vein, 0f, normalWidthVal),
          veinPoints);
        rendVeins.Add(rv);
      }

      return (pds, rendVeins, puffies);
    }

    public static MagickColor Color(string c) {
      return (MagickColor)new MagickColorFactory().Create(c);
    }

    public static MagickColor MC(Color c) {
      return (MagickColor)new MagickColorFactory().Create(c.ToHex());
    }

    public static string PropNameForTexType(TextureType t) {
      switch (t) {
        case TextureType.Albedo: return "_Albedo";
        case TextureType.Normal: return "_Normal";
        case TextureType.Height: return "_Height";
        case TextureType.VeinMask: return "_VeinMask";
        case TextureType.Clipping: return "_Clipping";
      }
      return "";
    }
  }

}
