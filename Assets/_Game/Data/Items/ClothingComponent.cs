using System;
using UnityEngine;

namespace Zomboid.Data.Items.Components
{
    public enum ClothingSlot
    {
        Head,
        Torso,
        Legs,
        Feet,
        Hands,
        Backpack
    }

    [Serializable]
    public class ClothingComponent : ItemComponent
    {
        public ClothingSlot Slot = ClothingSlot.Torso;
        public float ArmorBite = 0f;
        public float ArmorScratch = 0f;
        public float Warmth = 10f;
        public float WindResistance = 5f;
    }
}
