using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReskinSwitcherMod
{
    public static class ProcessedCollections
    {
        public static readonly Dictionary<string, tk2dSpriteCollectionData> processed = [];

        public static bool TryGetCollection(string name, out tk2dSpriteCollectionData coll)
        {
            if(processed.TryGetValue(name, out coll))
                return coll != null;

            coll = ETGMod.Assets.Collections.Find(x => x.name == name);

            if (coll == null)
                return false;

            processed[name] = coll;
            return true;
        }

        public static void ProcessNewCollection(tk2dSpriteCollectionData coll)
        {
            if (coll == null)
                return;

            if (processed.TryGetValue(coll.name, out var d) && d != null)
                return;

            processed[coll.name] = coll;

            foreach (var gr in ReskinLoader.groups.Values)
            {
                if (gr.TryGetReskin(gr.currentResprite, out var reskin))
                    reskin.ProcessCollection(coll, true);

                foreach (var r in gr.ReskinList)
                    r.ProcessCollection(coll, false);
            }
        }
    }
}
