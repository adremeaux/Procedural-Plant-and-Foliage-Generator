using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
public interface IStem {
  List<Curve3D> curves { get; set; }
  Vector3[] shape { get; set; }

  void CreateCurves(LeafParamDict fields, LeafDeps deps, ArrangementData arrData, FlowerPotController potController);

  float ShapeScaleAtPercent(float perc);

  bool IsEmpty();
}
}