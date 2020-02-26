using HarmonyLib;
using System.Reflection;
using Verse;

namespace Rimlaser
{
    [StaticConstructorOnStartup]
    public class Rimlaser
    {

        static Rimlaser()
        {
            var harmony = new Harmony("com.github.automatic1111.rimlaser");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
