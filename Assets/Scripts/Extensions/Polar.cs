using System;
using UnityEngine;

namespace BionicWombat {
public struct Polar {
  public static float Pi = 3.1415f;
  public static float Pi2 = Pi * 2f;
  public static float HalfPi = Pi / 2f;
  public static float Pi270 = Pi * 1.5f;
  public static float RadToDeg = 180f / Pi;
  public static float DegToRad = Pi / 180f;

  public float len;
  public float theta;
  public float deg => theta * RadToDeg;

  public Vector2 vec => new Vector2(len * (float)Math.Cos(theta), len * (float)Math.Sin(theta));

  public Polar(float len, float theta, bool withDeg = false) =>
    (this.len, this.theta) = (len, theta * (withDeg ? DegToRad : 1f));

  public override string ToString() => "(" + len + ", " + deg + "°)";
}

public struct Polar3 {
  public float len;
  public float lat;
  public float longi;
  public (float lat, float longi) deg => (lat * Polar.RadToDeg, longi * Polar.RadToDeg);

  public Polar3(float len, float lat, float longi, bool withDeg = false) {
    this.len = len;
    this.lat = lat * (withDeg ? Polar.DegToRad : 1f);
    this.longi = longi * (withDeg ? Polar.DegToRad : 1f);
  }

  public Vector3 Vector => new Vector3(
    len * Mathf.Sin(longi) * Mathf.Cos(lat),
    len * Mathf.Sin(longi) * Mathf.Sin(lat),
    len * Mathf.Cos(longi)
  );

  public override string ToString() => "(" + len + ", " + deg.lat + "°, " + deg.longi + "°)";
}
}