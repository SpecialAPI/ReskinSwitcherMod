using System;
using System.Collections.Generic;
using System.Linq;
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
        public Dictionary<Material, Texture> previousDefinitions = new Dictionary<Material, Texture>();

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
        public Dictionary<string, Tuple<Texture2D, RuntimeAtlasSegment>> definitionReplacements;
        public Dictionary<tk2dSpriteDefinition, DefinitionInfoCache> previousDefinitions = new Dictionary<tk2dSpriteDefinition, DefinitionInfoCache>();

        public override void ApplyResprites(tk2dSpriteCollectionData coll)
        {
            foreach (var kvp in definitionReplacements)
            {
                var def = coll.GetSpriteDefinition(kvp.Key);

                if (def != null)
                {
                    var replacement = kvp.Value.First;

                    var segment = kvp.Value.Second ??= ETGMod.Assets.Packer.Pack(replacement);

                    previousDefinitions[def] = new DefinitionInfoCache()
                    {
                        extractRegion = def.extractRegion,
                        flipped = def.flipped,
                        materialInst = def.materialInst,
                        texelSize = def.texelSize,
                        uvs = def.uvs,
                    };

                    def.flipped = tk2dSpriteDefinition.FlipMode.None;
                    def.materialInst = new Material(def.material);
                    def.texelSize = replacement.texelSize;
                    def.extractRegion = true;

                    def.materialInst.mainTexture = segment.texture;
                    def.uvs = segment.uvs;
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
        }
    }
}
