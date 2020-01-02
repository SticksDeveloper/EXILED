using System;
using EXILED;
using Grenades;
using Harmony;
using Mirror;
using UnityEngine;

namespace EXILED.Patches
{
	[HarmonyPatch(typeof(GrenadeManager), nameof(GrenadeManager.CallCmdThrowGrenade))]
	public class GrenadeThrownEvent
	{
		public static bool Prefix(ref GrenadeManager __instance, ref int id, ref bool slowThrow, ref double time)
		{
			if (EXILED.plugin.GrenadeThrownPatchDisable)
				return true;

			try
			{
				bool allow = true;
				Events.InvokeGrenadeThrown(ref __instance, ref id, ref slowThrow, ref time, ref allow);
				return allow;
			}
			catch (Exception e)
			{
				Plugin.Error($"Grenade thrown patch error: {e}");
				return true;
			}
		}
	}
}