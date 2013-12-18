using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

namespace RPG.Items
{
    [Serializable]
    public class PossibleDrop
    {
        public readonly Item Item;
        public readonly float Chance;

        public PossibleDrop(Item i, float c) {
            Item = i;
            Chance = c;
        }
    }
}
