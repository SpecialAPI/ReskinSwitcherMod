using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using UnityEngine;

namespace ReskinSwitcherMod
{
    public abstract class ReplacementBase
    {
        public string collName;
        public tk2dSpriteCollectionData loadedCollection;

        public abstract void ApplyResprites(tk2dSpriteCollectionData coll);
        public abstract void UnapplyResprites(tk2dSpriteCollectionData coll);
    }

    public class SpritesheetReplacement : ReplacementBase
    {
        public Texture2D spritesheet;
        public Dictionary<Material, Texture> previousDefinitions = [];

        public override void ApplyResprites(tk2dSpriteCollectionData coll)
        {
            if (coll.materials != null && coll.materials.Length > 0)
            {
                var material = coll.materials[0];

                if (material)
                {
                    var mainTexture = material.mainTexture;

                    if (mainTexture)
                    {
                        var atlasName = mainTexture.name;

                        if (!string.IsNullOrEmpty(atlasName))
                        {
                            if (atlasName[0] != '~')
                            {
                                spritesheet.name = '~' + atlasName;

                                for (int i = 0; i < coll.materials.Length; i++)
                                {
                                    if (coll.materials[i]?.mainTexture == null)
                                        continue;

                                    previousDefinitions[coll.materials[i]] = coll.materials[i].mainTexture;
                                    coll.materials[i].mainTexture = spritesheet;
                                }

                                coll.inst.materialInsts = null;
                                coll.inst.Init();

                                var instIsNew = coll.inst != coll;

                                if (instIsNew)
                                {
                                    if (coll.inst?.materials != null)
                                    {
                                        for (int i = 0; i < coll.inst.materials.Length; i++)
                                        {
                                            if (coll.inst.materials[i]?.mainTexture == null)
                                                continue;

                                            previousDefinitions[coll.inst.materials[i]] = coll.inst.materials[i].mainTexture;
                                            coll.inst.materials[i].mainTexture = spritesheet;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void UnapplyResprites(tk2dSpriteCollectionData coll)
        {
            foreach (var kvp in previousDefinitions)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.mainTexture = kvp.Value;
                }
            }

            previousDefinitions.Clear();
        }
    }

    public class IndividualReplacement : ReplacementBase
    {
        public Dictionary<string, DefinitionReplacementInfo> definitionReplacements;
        public Dictionary<tk2dSpriteDefinition, DefinitionInfoCache> previousDefinitions = [];

        public bool advanced;

        public override void ApplyResprites(tk2dSpriteCollectionData coll)
        {
            foreach (var kvp in definitionReplacements)
            {
                var def = coll.GetSpriteDefinition(kvp.Key);

                if (def != null)
                {
                    var inf = kvp.Value;

                    var replacement = inf.texture;
                    var segment = inf.pack ??= ETGMod.Assets.Packer.Pack(replacement);

                    if (segment == null)
                        continue;

                    previousDefinitions[def] = new DefinitionInfoCache()
                    {
                        extractRegion = def.extractRegion,
                        flipped = def.flipped,
                        materialInst = def.materialInst,
                        texelSize = def.texelSize,
                        uvs = def.uvs,

                        position0 = def.position0,
                        position1 = def.position1,
                        position2 = def.position2,
                        position3 = def.position3,

                        boundsCenter = def.boundsDataCenter,
                        boundsExtents = def.boundsDataExtents,
                        untrimmedBoundsCenter = def.untrimmedBoundsDataCenter,
                        untrimmedBoundsExtents = def.untrimmedBoundsDataExtents
                    };

                    def.flipped = tk2dSpriteDefinition.FlipMode.None;
                    def.materialInst = new Material(def.material);
                    def.texelSize = replacement.texelSize;
                    def.extractRegion = true;

                    def.materialInst.mainTexture = segment.texture;
                    def.uvs = segment.uvs;

                    if(advanced)
                    {
                        var origDimensions = def.position3 - def.position0;
                        var thisDimensions = new Vector3(replacement.width, replacement.height) / 16f;

                        var diff = thisDimensions - origDimensions;

                        var wDiffVector = new Vector3(diff.x / 2f, 0);
                        var hDiffVector = new Vector3(0, diff.y / 2f);

                        def.position0 += -wDiffVector - hDiffVector; // Expand the lower left corner to the left and down.
                        def.position1 += wDiffVector - hDiffVector; // Expand the lower right corner to the right and down.
                        def.position2 += -wDiffVector + hDiffVector; // Expand the upper left corner to the left and up.
                        def.position3 += wDiffVector + hDiffVector; // Expand the upper right corner to the right and up.

                        var c = replacement.GetPixels();

                        int? minX = null;
                        int? maxX = null;
                        int? minY = null;
                        int? maxY = null;

                        if (inf.hasSavedTrimData)
                        {
                            minX = inf.minX;
                            maxX = inf.maxX;
                            minY = inf.minY;
                            maxY = inf.maxY;
                        }
                        else if (replacement.IsReadable())
                        {
                            for (int i = 0; i < c.Length; i++)
                            {
                                var color = c[i];

                                if (color.a <= 0f)
                                    continue;

                                var x = i % replacement.width;
                                var y = i / replacement.width;

                                minY ??= y;
                                maxY = y;

                                minX = Mathf.Min(minX ?? int.MaxValue, x);
                                maxX = Mathf.Max(maxX ?? int.MinValue, x);
                            }
                        }

                        if (!inf.hasSavedTrimData)
                        {
                            inf.minX = minX;
                            inf.maxX = maxX;
                            inf.minY = minY;
                            inf.maxY = maxY;

                            inf.hasSavedTrimData = true;
                        }

                        if (minX != null && maxX != null && minY != null && maxY != null)
                        {
                            var trimmedWidth = maxX.GetValueOrDefault() - minX.GetValueOrDefault() + 1;
                            var trimmedHeight = maxY.GetValueOrDefault() - minY.GetValueOrDefault() + 1;

                            var trimmedDimensions = new Vector3(trimmedWidth, trimmedHeight) / 16f;
                            var center = new Vector3((maxX.GetValueOrDefault() + minX.GetValueOrDefault() + 1) / 2f, (maxY.GetValueOrDefault() + minY.GetValueOrDefault() + 1) / 2f) / 16f;

                            def.boundsDataCenter = def.position0 + center;
                            def.boundsDataExtents = trimmedDimensions;
                        }

                        else
                            def.boundsDataExtents += diff;

                        def.untrimmedBoundsDataExtents += diff;
                    }
                }
            }
        }

        public override void UnapplyResprites(tk2dSpriteCollectionData coll)
        {
            foreach (var kvp in previousDefinitions)
            {
                if (kvp.Key != null)
                {
                    var def = kvp.Key;
                    var cache = kvp.Value;

                    def.flipped = cache.flipped;
                    def.materialInst = cache.materialInst;
                    def.texelSize = cache.texelSize;
                    def.extractRegion = cache.extractRegion;
                    def.uvs = cache.uvs;

                    def.position0 = cache.position0;
                    def.position1 = cache.position1;
                    def.position2 = cache.position2;
                    def.position3 = cache.position3;

                    def.boundsDataCenter = cache.boundsCenter;
                    def.boundsDataExtents = cache.boundsExtents;
                    def.untrimmedBoundsDataCenter = cache.untrimmedBoundsCenter;
                    def.untrimmedBoundsDataExtents = cache.untrimmedBoundsExtents;
                }
            }

            previousDefinitions.Clear();
        }

        public class DefinitionInfoCache
        {
            public tk2dSpriteDefinition.FlipMode flipped;
            public Material materialInst;
            public Vector2 texelSize;
            public bool extractRegion;
            public Vector2[] uvs;

            public Vector3 position0;
            public Vector3 position1;
            public Vector3 position2;
            public Vector3 position3;

            public Vector3 boundsCenter;
            public Vector3 boundsExtents;
            public Vector3 untrimmedBoundsCenter;
            public Vector3 untrimmedBoundsExtents;
        }

        public class DefinitionReplacementInfo
        {
            public Texture2D texture;
            public RuntimeAtlasSegment pack;

            public bool hasSavedTrimData;
            public int? minX;
            public int? maxX;
            public int? minY;
            public int? maxY;
        }
    }
}
