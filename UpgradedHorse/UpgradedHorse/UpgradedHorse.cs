using System;

namespace UpgradedHorseMod
{
    public class UpgradedHorse
    {
        public String displayName;
        public HorseData horseData;

        public UpgradedHorse(String displayName, HorseData horseData)
        {
            this.displayName = displayName;
            this.horseData = horseData;
        }

        public string getMoodMessage()
        {
            return "Neigh";
        }
    }
}
