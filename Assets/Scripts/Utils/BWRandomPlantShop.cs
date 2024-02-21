namespace BionicWombat {
  public static class BWRandomPlantShop {
    public static int GenTypedSeed(int baseSeed, LPType lpType = LPType.Leaf, int leafIndex = 0) {
      return baseSeed + leafIndex + ((int)lpType * 100);
    }
  }
}
