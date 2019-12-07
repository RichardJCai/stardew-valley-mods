using System;
using StardewValley.Characters;

namespace UpgradedHorseMod
{
    public class UpgradedHorse : Horse
    {
        public double friendshipTowardFarmer;
        public double fullness;
        public double happiness;

        public UpgradedHorse(Horse horse)
        {
            this.displayName = horse.displayName;
            this.friendshipTowardFarmer = 1000;
            this.fullness = 500;
            this.happiness = 500;
        }

        public string getMoodMessage()
        {
            return "Neigh";
        }
    }
}
