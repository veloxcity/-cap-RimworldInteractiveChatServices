// JobDefOf_CAP.cs
using RimWorld;
using Verse;

namespace CAP_ChatInteractive
{
    [DefOf]
    public static class JobDefOf_CAP
    {
        public static JobDef CAP_SocialVisit;

        static JobDefOf_CAP()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf_CAP));
        }
    }
}