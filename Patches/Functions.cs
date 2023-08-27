namespace OdinUndercroft.Patches;

public static class Functions
{
    internal static void RegisterAllSFX()
    {
        // Right now this is only one SFX, but it's here for future use to easily add more.
        PieceManager.PiecePrefabManager.RegisterPrefab("odins_undercroft", "SFX_Trap_Destroyed");
    }
}