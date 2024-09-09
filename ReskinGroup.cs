using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReskinSwitcherMod
{
    public class ReskinGroup
    {
        public string name;
        public string currentResprite;

        public RandomReskin randomReskin;
        public EmptyReskin emptyReskin;

        public Dictionary<string, NamedReskin> reskins;

        public List<ReskinBase> ReskinList => [emptyReskin, .. reskins.Values, randomReskin];
        public List<ReskinBase> ReskinListNoRandom => [emptyReskin, .. reskins.Values];

        public ReskinGroup()
        {
            randomReskin = new() { group = this };
            emptyReskin = new() { group = this };
        }

        public bool TryGetReskin(string name, out ReskinBase reskin)
        {
            if (name == ReskinConfig.NoResprite)
            {
                reskin = null;
                return false;
            }

            if (name == ReskinConfig.RandomResprite)
            {
                reskin = randomReskin;
                return true;
            }

            if (reskins.TryGetValue(name, out var named))
            {
                reskin = named;
                return true;
            }

            reskin = null;
            return false;
        }
    }
}
