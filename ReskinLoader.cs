using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ReskinSwitcherMod
{
    public static class ReskinLoader
    {
        public const string RESKIN_FILTER = "*-resprite.spapi";

        public const string READMODE_GROUPNAME = "GroupName";
        public const string READMODE_RESPRITENAME = "RespriteName";
        public const string READMODE_RESPRITEFILES = "RespriteFiles";

        public static Dictionary<string, ReskinGroup> groups = [];

        public static void LoadReskins()
        {
            var files = Directory.GetFiles(Paths.PluginPath, RESKIN_FILTER, SearchOption.AllDirectories);

            if (files == null)
                return;

            foreach (var f in files)
            {
                var fname = Path.GetFileName(f);
                Debug.Log($"[{Plugin.NAME}] Reading resprite file: {fname}");

                if (!TryReadReskinData(f, out var groupName, out var replacementName, out var replacements))
                    continue;                

                if (groupName.IsNullOrWhiteSpace() || replacementName.IsNullOrWhiteSpace() || replacements.Count <= 0)
                {
                    Debug.LogError($"Error reading resprite data file \"{fname}\": {READMODE_GROUPNAME}, {READMODE_RESPRITENAME} and/or {READMODE_RESPRITEFILES} are empty.");
                    continue;
                }

                if (replacementName == ReskinConfig.NoResprite || replacementName == ReskinConfig.RandomResprite)
                {
                    Debug.LogError($"Error reading resprite data file \"{fname}\": RespriteName cannot be a special name ({replacementName}).");
                    continue;
                }

                var r = ReadReplacementsForReskin(f, replacements);

                if (r.Count <= 0)
                    continue;

                var replacement = new NamedReskin() { _name = replacementName, replacements = r };

                if (!groups.TryGetValue(groupName, out var group))
                    groups[groupName] = group = new ReskinGroup() { name = groupName, reskins = [] };

                try
                {
                    group.reskins.Add(replacementName, replacement);
                    replacement.group = group;

                    Debug.Log($"Successfully added resprite \"{replacementName}\" to group \"{groupName}\"");
                }
                catch
                {
                    Debug.LogError($"Error loading resprite data file \"{fname}\": resprite \"{replacementName}\" already exists.");
                }
            }
        }

        public static List<ReplacementBase> ReadReplacementsForReskin(string f, List<string> replacements)
        {
            var fname = Path.GetFileName(f);
            var fld = Path.GetDirectoryName(f);

            var r = new List<ReplacementBase>();

            foreach (var a in replacements)
            {
                var p = Path.Combine(fld, a);
                var c = Path.GetFileNameWithoutExtension(a);

                if (a.EndsWith(".png"))
                {
                    if (!File.Exists(p))
                    {
                        Debug.LogError($"Error reading resprites for resprite data file \"{fname}\": no file \"{a}\" exists in folder \"{Path.GetFileName(fld)}\".");
                        continue;
                    }

                    if(!TryReadImage(p, out var tx))
                    {
                        Debug.LogError($"Error reading spritesheet resprite \"{a}\" for resprite data file \"{fname}\": invalid image.");
                        continue;
                    }

                    r.Add(new SpritesheetReplacement() { spritesheet = tx, collName = c });
                    continue;
                }

                if (!Directory.Exists(p))
                {
                    Debug.LogError($"Error reading resprites for resprite data file \"{fname}\": no folder \"{a}\" exists in folder \"{Path.GetFileName(fld)}\".");
                    continue;
                }

                var rp = new IndividualReplacement() { collName = c, definitionReplacements = [] };

                foreach (var d in Directory.GetFiles(p, "*.png"))
                {
                    if (!TryReadImage(d, out var tx))
                    {
                        Debug.LogError($"Error reading individual resprite frame \"{a}/{Path.GetFileName(d)}\" for resprite data file \"{fname}\": invalid image.");
                        continue;
                    }

                    if (!rp.definitionReplacements.ContainsKey(tx.name))
                        rp.definitionReplacements[tx.name] = new Tuple<Texture2D, RuntimeAtlasSegment>(tx, null);

                    else
                        Debug.LogError($"Error reading individual resprite frame \"{a}/{Path.GetFileName(d)}\" for resprite data file \"{fname}\": frame named \"{tx.name}\" already exists.");
                }

                if (rp.definitionReplacements.Count > 0)
                    r.Add(rp);
            }

            return r;
        }

        public static bool TryReadImage(string f, out Texture2D tex)
        {
            tex = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                name = Path.GetFileNameWithoutExtension(f)
            };

            try
            {
                if (tex.LoadImage(File.ReadAllBytes(f)))
                    return true;
            }
            catch { }

            return false;
        }

        public static bool TryReadReskinData(string f, out string groupName, out string replacementName, out List<string> replacements)
        {
            var fname = Path.GetFileName(f);

            var readingReplacements = false;
            var readingGroupName = false;
            var readingReplacementName = false;

            groupName = "";
            replacementName = "";
            replacements = [];

            foreach (var l_ in File.ReadAllLines(f))
            {
                if (l_.IsNullOrWhiteSpace())
                    continue;

                var l = l_.Trim();

                if (l.StartsWith("#") && l.Length > 1)
                {
                    var readmode = l.Substring(1).Trim().ToLowerInvariant();

                    if (readmode.IsNullOrWhiteSpace())
                        continue;

                    readingGroupName = false;
                    readingReplacementName = false;
                    readingReplacements = false;

                    if (readmode == READMODE_GROUPNAME.ToLowerInvariant())
                        readingGroupName = true;

                    else if (readmode == READMODE_RESPRITENAME.ToLowerInvariant())
                        readingReplacementName = true;

                    else if (readmode == READMODE_RESPRITEFILES.ToLowerInvariant())
                        readingReplacements = true;

                    else
                    {
                        Debug.LogError($"Error reading resprite data file \"{fname}\": read mode \"{readmode}\" doesn't exist.");

                        return false;
                    }
                }
                else
                {
                    if (readingGroupName)
                        groupName = l;

                    else if (readingReplacementName)
                        replacementName = l;

                    else if (readingReplacements)
                        replacements.Add(l);

                    else
                    {
                        Debug.LogError($"Error reading resprite data file \"{fname}\": unexpected line \"{l_}\" before read mode definition.");

                        return false;
                    }
                }
            }

            return true;
        }
    }
}
