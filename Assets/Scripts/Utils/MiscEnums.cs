using System;

namespace BionicWombat {
  public enum AssignmentState {
    Unassigned,
    Assigned,
    Disabled
  }

  public enum ButtonState {
    Active,
    MouseDown,
    Disabled,
  }

  public enum Trinary {
    t0,
    t1,
    t2
  }

  public enum UseState {
    None,
    Running,
    Completed,
  }

  public enum BlendMode {
    Overwrite,
    Darken,
    Multiply,
    ColorBurn,
    LinearBurn,
    Lighten,
    Screen,
    ColorDodge,
    LinearDodge,
    Overlay,
    SoftLight,
    HardLight,
    VividLight,
    LinearLight,
    PinLight,
    HardMix,
    Difference,
    Exclusion,
    Subtract,
    Divide
  }
}
