namespace CopperBend.Contract
{
    public enum DamageType
    {
        Unset = 0,
        Not_otherwise_specified,  //0.2: Use this type as fallback
        Physical_blunt_hit,
        Physical_edge_hit,
        Physical_point_hit,
        Physical_shear,
        Fire,
        Lightning,
        Light,
        Water,
        Nature_plant,
        Nature_itself,
        Nature_toxin,
        Blight_spark,
        Blight_toxin,
    }
    //1.+: Add categories, perhaps physical, energetic, magical, and vital.
}
