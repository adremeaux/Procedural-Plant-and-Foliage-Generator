using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BionicWombat {
  public class ParticleSystemSmokePuff : MonoBehaviour {
    public ParticleSystem front;
    public ParticleSystem back;
    public ParticleSystem left;
    public ParticleSystem right;

    public void RandomSize() {
      SetSize(BWRandom.UnseededRange(0.5f, 2f), BWRandom.UnseededRange(0.5f, 2f));
    }

    public void SetSize(float w, float h) {
      (var f, var b, var l, var r) = (front.shape, back.shape, left.shape, right.shape);
      f.radius = b.radius = w;
      l.radius = r.radius = h;
      f.position = new Vector3(0f, 0f, -h);
      b.position = -f.position;
      l.position = new Vector3(w, 0f, 0f);
      r.position = -l.position;
    }

    public void Play() {
      front.Play();
      back.Play();
      left.Play();
      right.Play();
    }

    public void Stop() {
      front.Stop();
      back.Stop();
      left.Stop();
      right.Stop();
    }
  }
}
