﻿/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace OdinUndercroft.Patches
{
    [Harmony]
    static class Player_Patches
    {
        const float overlapRadius = 60;
        
        [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
        [HarmonyPostfix]
        public static void Player_UpdatePlacementGhost(Player __instance, GameObject ___m_placementGhost)
        {
            if (!___m_placementGhost) return;
            var basementComponent = ___m_placementGhost.GetComponent<Basement>();
            if (!basementComponent) return;
            if (Basement.allBasements.Count <= 0) return;
            Type type = typeof(Player).Assembly.GetType("Player+PlacementStatus");
            object moreSpace = type.GetField("MoreSpace").GetValue(__instance);
            FieldInfo statusField = __instance.GetType().GetField("m_placementStatus", BindingFlags.NonPublic | BindingFlags.Instance);


            foreach (var basement in Basement.allBasements)
            {
                float dist = Vector3.Distance(basement.transform.position, Player.m_localPlayer.transform.position);
                if (basement.mUID == basementComponent.mUID)
                {
                    continue;
                }
                if (dist <= overlapRadius)
                {
                    statusField.SetValue(__instance, moreSpace); 
        
                }
            }

        }

        [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            /*
            158 ldfld System.Boolean m_groundPiece
            159 brfalse System.Reflection.Emit.Label
            160 ldloc.s Heightmap (5)
            161 ldnull
            162 call Boolean op_Equality(UnityEngine.Object, UnityEngine.Object)
            163 brfalse System.Reflection.Emit.Label
            164 ldarg.0
            165 ldfld UnityEngine.GameObject m_placementGhost
            166 ldc.i4.0
            167 callvirt Void SetActive(Boolean)
            168 ldarg.0
            169 ldc.i4.1
            170 stfld Player+PlacementStatus m_placementStatus
            171 ret
            172 ldloc.1
            173 ldfld System.Boolean m_groundOnly
            174 brfalse System.Reflection.Emit.Label
            175 ldloc.s Heightmap (5)
            176 ldnull
            177 call Boolean op_Equality(UnityEngine.Object, UnityEngine.Object)
            178 brfalse System.Reflection.Emit.Label
            //
            codes[164] = CodeInstruction.Call(typeof(Player_Patches), "OverrideNullEqualityInBasement");
            codes[189]= CodeInstruction.Call(typeof(Player_Patches), "OverrideNullEqualityInBasement");
            return codes.AsEnumerable();
        }

        static bool OverrideNullEqualityInBasement(UnityEngine.Object a, UnityEngine.Object b)
        {
            if (EnvMan.instance.GetCurrentEnvironment().m_name == "Basement")
            {
                return false;
            }
            return a == b;
        }
    }
}
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace OdinUndercroft.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
    static class Player_UpdatePlacementGhost_Patch
    {
        const float overlapRadius = 60;

        static void Postfix(Player __instance, GameObject ___m_placementGhost)
        {
            if (!___m_placementGhost) return;
            var basementComponent = ___m_placementGhost.GetComponent<Basement>();
            if (!basementComponent) return;
            if (Basement.allBasements.Count <= 0) return;
            Type type = typeof(Player).Assembly.GetType("Player+PlacementStatus");
            object moreSpace = type.GetField("MoreSpace").GetValue(__instance);
            FieldInfo statusField = __instance.GetType().GetField("m_placementStatus", BindingFlags.NonPublic | BindingFlags.Instance);
            var ol = Basement.allBasements.Where(x => Vector3.Distance(x.transform.position, ___m_placementGhost.transform.position) < overlapRadius).Where(x => x.gameObject != ___m_placementGhost);
            if (ol.Any(x => x.GetComponentInParent<Basement>()) || ___m_placementGhost.transform.position.y > 2500 * Mathf.Max(OdinUndercroftPlugin.MaxNestedLimit.Value, 0) + 2000)
            {
                statusField.SetValue(__instance, moreSpace);
            }
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpdatePlacementGhostTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(
                    useEnd: false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Piece), nameof(Piece.m_groundPiece))),
                    new CodeMatch(OpCodes.Brfalse),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldnull),
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(UnityEngine.Object), "op_Equality", new Type[] { typeof(UnityEngine.Object), typeof(UnityEngine.Object) })))
                .Advance(offset: 5)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(HeightmapIsNullBasemementDelegate))
                .MatchForward(
                    useEnd: false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Piece), nameof(Piece.m_groundOnly))),
                    new CodeMatch(OpCodes.Brfalse),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldnull),
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(UnityEngine.Object), "op_Equality", new Type[] { typeof(UnityEngine.Object), typeof(UnityEngine.Object) })))
                .Advance(offset: 5)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(HeightmapIsNullBasemementDelegate))
                .InstructionEnumeration();
        }

        static bool HeightmapIsNullBasemementDelegate(bool isEqual)
        {
            if (EnvMan.s_instance.GetCurrentEnvironment().m_name == "Basement")
            {
                return false;
            }

            return isEqual;
        }
    }
}