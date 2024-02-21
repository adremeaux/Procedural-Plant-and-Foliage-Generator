using ImageMagick;
using UnityEngine;

namespace BionicWombat {
  public class IMTextureCmdDrawTestNormals : BaseTextureCommand, IMTextureCommand, IMPixelCommand {
    public void Prepare() { }

    public void Composite(MagickImage image) { }

    public PixelFillCommand[] GetPixelFillCommands() {
      MarkStart();
      int w = 200;
      PixelFillCommand s1 = new PixelFillCommand();
      s1.colors = NormalDrawing.DrawSphere(w, 1f, 0.5f);
      s1.rect = new RectInt(w, w, w, w);
      s1.useAlpha = true;

      PixelFillCommand s2 = new PixelFillCommand();
      s2.colors = NormalDrawing.DrawSphere(w, 0.5f);
      s2.rect = new RectInt(w * 2, w, w, w);
      s2.useAlpha = true;

      PixelFillCommand s4 = new PixelFillCommand();
      s4.colors = NormalDrawing.DrawSphere(w, 0.125f);
      s4.rect = new RectInt(w, w * 2, w, w);
      s4.useAlpha = true;

      PixelFillCommand s3 = new PixelFillCommand();
      s3.colors = NormalDrawing.DrawSphere(w, 2f);
      s3.rect = new RectInt(w * 2, w * 2, w, w);
      s3.useAlpha = true;


      // PixelFillCommand c1 = new PixelFillCommand();
      // c1.colors = NormalDrawing.DrawCylinder(w / 2, w, 0f, 1f).colors;
      // c1.rect = new RectInt(w * 3, w, w / 2, w);

      // PixelFillCommand c2 = new PixelFillCommand();
      // c2.colors = NormalDrawing.DrawCylinder(w, w / 2, 0f, 1f).colors;
      // c2.rect = new RectInt(w * 3, w * 2, w, w / 2);

      // PixelFillCommand c3 = new PixelFillCommand();
      // c3.colors = NormalDrawing.DrawCylinder(w / 2, w, 0f, .5f).colors;
      // c3.rect = new RectInt((int)((float)w * 3.5f), w, w / 2, w);

      // PixelFillCommand c4 = new PixelFillCommand();
      // c4.colors = NormalDrawing.DrawCylinder(w, w / 2, 0f, 2f).colors;
      // c4.rect = new RectInt(w * 3, (int)((float)w * 2.5f), w, w / 2);

      PixelFillCommand DrawCyl(bool tall, float deg, float x, float y) {
        PixelFillCommand r = new PixelFillCommand();
        RectInt newRect;
        int halfW = w / 2;
        (r.colors, newRect) = NormalDrawing.DrawCylinder(tall ? halfW / 2 : halfW, !tall ? halfW / 2 : halfW, deg * Polar.DegToRad);
        r.rect = new RectInt((int)(w * x), (int)(w * y), newRect.width, newRect.height);
        r.useAlpha = true;
        return r;
      }

      PixelFillCommand r1 = DrawCyl(true, 0f, 1.25f, 3);
      PixelFillCommand r2 = DrawCyl(true, 90f, 1.5f, 3);
      PixelFillCommand r3 = DrawCyl(true, 180f, 1, 3.5f);
      PixelFillCommand r4 = DrawCyl(true, 270f, 1.5f, 3.5f);

      PixelFillCommand r5 = DrawCyl(true, 45f, 2, 3);
      PixelFillCommand r6 = DrawCyl(true, 135f, 2.5f, 3);
      PixelFillCommand r7 = DrawCyl(true, 225f, 2, 3.5f);
      PixelFillCommand r8 = DrawCyl(true, 315f, 2.5f, 3.5f);

      PixelFillCommand r9 = DrawCyl(true, 18f, 3, 3);
      PixelFillCommand r10 = DrawCyl(true, 36f, 3.5f, 3);
      PixelFillCommand r11 = DrawCyl(true, 54f, 3, 3.5f);
      PixelFillCommand r12 = DrawCyl(true, 72f, 3.5f, 3.5f);

      PixelFillCommand DrawQuad(float x, float y, float a1, float a2, float a3, float a4,
          float nW, bool flip = false, float flipPoint = 0.55f) {
        PixelFillCommand p1 = new PixelFillCommand();
        RectInt newRect;
        Vector2 origin = new Vector2(w * 3, w * 2);
        Vector2[] vec = new Vector2[4] {
        origin + new Vector2(nW * 0f, nW * a1),
        origin + new Vector2(nW * a2, nW * 0f),
        origin + new Vector2(nW * 1f, nW * a3),
        origin + new Vector2(nW * a4, nW * 1f),
      };
        if (flip) {
          Vector2 temp = vec[0];
          vec[0] = vec[1]; vec[1] = vec[2]; vec[2] = vec[3]; vec[3] = temp;
        }
        (p1.colors, newRect) = NormalDrawing.DrawQuad(vec, 1f, flipPoint);
        p1.rect = new RectInt((int)(w * x), (int)(w * y), newRect.width, newRect.height);
        p1.useAlpha = true;
        return p1;
      }

      PixelFillCommand p1 = DrawQuad(3, 1, .9f, .1f, .9f, .9f, w / 2, false, 0.5f);
      PixelFillCommand p11 = DrawQuad(3.5f, 1, .1f, .9f, .9f, .9f, w / 2, true);
      PixelFillCommand p2 = DrawQuad(3, 1.5f, .5f, .5f, .5f, .5f, w / 2);
      PixelFillCommand p21 = DrawQuad(3.5f, 1.5f, .5f, .5f, .5f, .5f, w / 2, true);
      PixelFillCommand p3 = DrawQuad(3, 2, .1f, .5f, .5f, .25f, w);

      MarkEnd();
      return new PixelFillCommand[] {
      s1, s2, s3, s4,
      // c1, c2, c3, c4,
      r1, r2, r3, r4,
      r5, r6, r7, r8,
      r9, r10, r11, r12,
      p1, p2, p3, p11, p21
    };
    }
  }
}
