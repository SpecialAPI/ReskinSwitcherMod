using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ReskinSwitcherMod
{
    public abstract class ReskinBase
    {
        public List<ReplacementBase> replacements = [];
        public ReskinGroup group;
        
        public abstract string Name { get; }

        public virtual void ApplyReplacements()
        {
            foreach (var r in replacements)
            {
                if (r.loadedCollection == null)
                    continue;

                r.ApplyResprites(r.loadedCollection);
            }
        }

        public virtual void UnapplyReplacements()
        {
            foreach (var r in replacements)
            {
                if (r.loadedCollection == null)
                    continue;

                r.UnapplyResprites(r.loadedCollection);
            }
        }

        public virtual void LoadCollections(bool alsoApply)
        {
            foreach (var r in replacements)
            {
                if(!ProcessedCollections.TryGetCollection(r.collName, out r.loadedCollection))
                    continue;

                if (alsoApply)
                    r.ApplyResprites(r.loadedCollection);
            }
        }

        public virtual void ProcessCollection(tk2dSpriteCollectionData coll, bool alsoApply)
        {
            if(coll == null)
                return;

            foreach (var r in replacements)
            {
                if (r.loadedCollection != null || r.collName != coll.name)
                    continue;

                r.loadedCollection = coll;

                if (alsoApply)
                    r.ApplyResprites(coll);
            }
        }

        public virtual void OnNewRunStarted()
        {
        }
    }

    public class NamedReskin : ReskinBase
    {
        public string _name;

        public override string Name => _name;
    }

    public class EmptyReskin : ReskinBase
    {
        public override string Name => ReskinConfig.NoResprite;

        public override void ApplyReplacements()
        {
        }

        public override void UnapplyReplacements()
        {
        }

        public override void LoadCollections(bool alsoApply)
        {
        }

        public override void ProcessCollection(tk2dSpriteCollectionData coll, bool alsoApply)
        {
        }
    }

    public class RandomReskin : ReskinBase
    {
        public override string Name => ReskinConfig.RandomResprite;

        public override void ApplyReplacements()
        {
            RandomizeReplacements();
            base.ApplyReplacements();
        }

        public override void LoadCollections(bool alsoApply)
        {
            RandomizeReplacements();
            base.LoadCollections(alsoApply);
        }

        public override void OnNewRunStarted()
        {
            UnapplyReplacements();
            ApplyReplacements();
        }

        public void RandomizeReplacements()
        {
            replacements.Clear();

            if (group == null)
                return;

            var l = group.ReskinListNoRandom;

            if (l == null || l.Count <= 0)
                return;

            replacements.AddRange(BraveUtility.RandomElement(group.ReskinListNoRandom).replacements);
        }
    }
}
