using System;
using StardewValley.Characters;

namespace UpgradedHorseMod
{
    public class UpgradedHorse
    {
        public String displayName;
        public double friendship;
        public double fullness;
        public double happiness;

        public UpgradedHorse(String displayName, double friendship, double fullness,
            double happiness)
        {
            this.displayName = displayName;
            this.friendship = friendship;
            this.fullness = fullness;
            this.happiness = happiness;
        }

        public string getMoodMessage()
        {
            return "Neigh";
        }
    }
}
