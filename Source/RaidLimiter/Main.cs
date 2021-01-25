using System.Reflection;
using HarmonyLib;
using Verse;

namespace RaidLimiter
{
    // Token: 0x02000002 RID: 2
    [StaticConstructorOnStartup]
	internal class Main
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		static Main()
		{
			var harmonyInstance = new Harmony("com.whocares.whatisthis.rimworld.mod.stuff");
			harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}
