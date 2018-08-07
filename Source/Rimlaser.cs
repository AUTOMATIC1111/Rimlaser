using Harmony;
using System.Reflection;
using Verse;

namespace Rimlaser
{
    [StaticConstructorOnStartup]
    public class Rimlaser
    {

        static Rimlaser()
        {
            var harmony = HarmonyInstance.Create("com.github.automatic1111.rimlaser");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
