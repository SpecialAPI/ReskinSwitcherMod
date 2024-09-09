using Gunfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReskinSwitcherMod
{
    public static class ReskinConfig
    {
        public static Gunfig gunfig;

        public static string NoResprite = "None";
        public static string RandomResprite = "Random";

        public static void Init()
        {
            if (ReskinLoader.groups.Count <= 0)
                return;

            gunfig = Gunfig.Get(Plugin.NAME);

            foreach (var gr in ReskinLoader.groups.Values)
            {
                gunfig.AddScrollBox(
                    key: gr.name,
                    options: gr.ReskinList.ConvertAll(x => x.Name),
                    callback: MaybeUpdateSprites,
                    label: $"Current resprite for group \"{gr.name}\"");

                gr.currentResprite = gunfig.Value(gr.name);

                foreach (var r in gr.ReskinList)
                    r.LoadCollections(r.Name == gr.currentResprite);
            }
        }

        public static void MaybeUpdateSprites(string key, string value)
        {
            if (!ReskinLoader.groups.TryGetValue(key, out var group))
                return;

            if (group.currentResprite != null && group.TryGetReskin(group.currentResprite, out var curr))
                curr.UnapplyReplacements();

            if (group.TryGetReskin(value, out var replacement))
                replacement.ApplyReplacements();

            group.currentResprite = value;
        }
    }
}
