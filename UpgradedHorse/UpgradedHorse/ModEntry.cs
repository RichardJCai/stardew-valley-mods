using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Characters;
using StardewValley;
using System.Linq;
using StardewValley.Menus;


namespace UpgradedHorseMod
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        static bool horseFed = false;
        static bool isHorseSpeedAdded = false;

        public static int openMenuX;
        public static int openMenuY;

        public const int INVENTORY_TAB = 0;

        private int addedHorseSpeed = 0;
        private int currentAddedSpeed = 0;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.Saving += this.OnSaving;
        }

        ///// <summary>The method called after a new day starts.</summary>
        ///// <param name="sender">The event sender.</param>
        ///// <param name="e">The event arguments.</param>
        ///
        /*********
        ** Private methods
        *********/

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Game1.currentLocation.IsFarm)
            {
                return;
            }

            if (e.Button == SButton.MouseRight)
            {
                Point cursorPosition = Game1.getMousePosition();
                FeedHorse(Game1.currentLocation,
                    cursorPosition.X + Game1.viewport.X,
                    cursorPosition.Y + Game1.viewport.Y);
            }

            if (e.Button == SButton.MouseLeft)
            {
                Point cursorPosition = Game1.getMousePosition();
                OpenHorseMenu(cursorPosition.X,
                          cursorPosition.Y);
            }

            // Read button from CONFIG
            if (e.Button == SButton.H)
            {
                OpenHorseMenu();
            }
        }

        private void OnDayStarted(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady || !Game1.currentLocation.IsFarm)
            {
                return;
            }

            horseFed = false;
            isHorseSpeedAdded = false;

            // Migration

            HorseData horseData = LoadHorseDataForPlayer(Game1.player.name);
            if (horseData == null)
            {
                // Read Global for migration from 1.0.0
                horseData = Helper.Data.ReadGlobalData<HorseData>(
                String.Format("{0}-horse-data", Game1.player.name) // Not sure if player name is unique
                );
                if (horseData == null)
                {
                    horseData = new HorseData(0, false);
                }
            }

            horseData.Full = false;
            addedHorseSpeed = horseData.Friendship / 200;

            SaveTempHorseDataForPlayer(Game1.player.name, horseData);
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree || !Context.IsWorldReady || Game1.paused
                || Game1.activeClickableMenu != null)
                return;

            UpdateAddedSpeed();
        }


        // Speed boost depends on how much your horse loves you
        private void UpdateAddedSpeed()
        {
            int speedbuff = 0;

            // Handle case where player has an existing speed buff
            // currentAddedSpeed is only speed for horse buff
            if (Game1.player.addedSpeed > currentAddedSpeed)
            {
                speedbuff = Game1.player.addedSpeed - currentAddedSpeed;
            }
            if (Game1.player.mount != null && !isHorseSpeedAdded && horseFed)
            {
                isHorseSpeedAdded = true;
                currentAddedSpeed = addedHorseSpeed;
            }
            else if (Game1.player.mount == null && isHorseSpeedAdded)
            {
                isHorseSpeedAdded = false;
                currentAddedSpeed = 0;
            }

            Game1.player.addedSpeed = currentAddedSpeed + speedbuff;
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs args)
        {
            if (args.NewMenu is GameMenu gm)
            {
                openMenuX = gm.xPositionOnScreen;
                openMenuY = gm.yPositionOnScreen;
            }
            else if (args.OldMenu is GameMenu ogm)
            {
                openMenuX = ogm.xPositionOnScreen;
                openMenuY = ogm.yPositionOnScreen;
            }
        }


        private void OnSaving(object sender, SavingEventArgs e)
        {
            // Same Temp Horse Data to Global On Save
            HorseData horseData = LoadTempHorseDataForPlayer(Game1.player.name);
            SaveHorseDataForPlayer(Game1.player.name, horseData);

        }


        private void FeedHorse(GameLocation currentLocation, int x, int y)
        {
            if (Game1.player.CurrentItem == null)
            {
                return;
            }

            // Find if click was on Horse
            foreach (Horse horse in currentLocation.characters.OfType<Horse>())
            {
                // Can only feed your own horse
                if (horse.getOwner() != Game1.player)
                {
                    return;
                }

                if (Utility.withinRadiusOfPlayer((int)(horse.Position.X), (int)(horse.Position.Y), 1, Game1.player)
                    && (Utility.distance((float)x, horse.Position.X, (float)y, horse.Position.Y) <= 100))
                {
                    // Holding food
                    if (Game1.player.CurrentItem.category == -7)
                    {
                        if (horseFed == false)
                        {
                            Item food = Game1.player.CurrentItem;
                            Game1.drawObjectDialogue(string.Format("{0} ate your {1}.", horse.name, food.Name));
                            Game1.player.reduceActiveItemByOne();
                            horseFed = true;


                            HorseData horseData = LoadTempHorseDataForPlayer(Game1.player.name);


                            if (horseData == null)
                            {
                                horseData = new HorseData(10, true);
                            }

                            horseData.Friendship += 10;
                            horseData.Full = true;

                            // Update addedHorseSpeed if Friendship increases enough
                            addedHorseSpeed = horseData.Friendship / 200;

                            SaveTempHorseDataForPlayer(Game1.player.name, horseData);
                        }
                        else
                        {
                            Game1.drawObjectDialogue(string.Format("{0} is full.", horse.name));
                        }
                    }
                }
            }
        }

        private void OpenHorseMenu(Nullable<int> x = null, Nullable<int> y =null)
        {
            if (x == null && y == null)
            {
                String horseName = Game1.player.horseName;

                if (horseName == null) return;

                HorseData horseData = LoadTempHorseDataForPlayer(Game1.player.name);

                if (horseData == null)
                {
                    horseData = new HorseData(0, false);
                }

                UpgradedHorse horse = new UpgradedHorse(
                    horseName, horseData
                );

                Game1.activeClickableMenu = (IClickableMenu)new HorseMenu(horse, this);
                return;
            }

            if (Game1.activeClickableMenu is GameMenu menu)
            {
                if (menu.currentTab == INVENTORY_TAB)
                {
                    Vector2 rectangle = new Vector2((float)((double)(openMenuX + Game1.tileSize * 5 + Game1.pixelZoom * 2) + (double)Math.Max((float)Game1.tileSize, Game1.dialogueFont.MeasureString(Game1.player.name).X / 2f) + (Game1.player.getPetDisplayName() != null ? (double)Math.Max((float)Game1.tileSize, Game1.dialogueFont.MeasureString(Game1.player.getPetDisplayName()).X) : 0.0)), (float)(openMenuY + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 7 * Game1.tileSize - Game1.pixelZoom));
                    if (Utility.distance((float)x, rectangle.X, (float)y, rectangle.Y) <= 100)
                    {
                        String horseName = Game1.player.horseName;

                        if (horseName == null) return;

                        HorseData horseData = LoadTempHorseDataForPlayer(Game1.player.name);

                        if (horseData == null)
                        {
                            horseData = new HorseData(0, false);
                        }

                        UpgradedHorse horse = new UpgradedHorse(
                            horseName, horseData
                        );

                        Game1.activeClickableMenu = (IClickableMenu)new HorseMenu(horse, this);
                        return;
                    }
                }
            }
        }
        // Need to handle migration of data

        // Not sure if player name is unique
        public HorseData LoadHorseDataForPlayer(string player)
        {
            return this.Helper.Data.ReadSaveData<HorseData>(
                String.Format("{0}-horse-data", player) // Not sure if player name is unique
                );
        }


        public void SaveHorseDataForPlayer(string player, HorseData horseData)
        {
            this.Helper.Data.WriteSaveData<HorseData>(
                String.Format("{0}-horse-data", player), horseData
                );
        }

        public HorseData LoadTempHorseDataForPlayer(string player)
        {
            return this.Helper.Data.ReadSaveData<HorseData>(
                String.Format("{0}-horse-data-temp", player) // Not sure if player name is unique
                );
        }

        public void SaveTempHorseDataForPlayer(string player, HorseData horseData)
        {
            this.Helper.Data.WriteSaveData<HorseData>(
                String.Format("{0}-horse-data-temp", player), horseData
                );
        }
    }

}

public class HorseData
{
    public HorseData(int friendship, bool full)
    {
        Friendship = friendship;
        Full = full;
    }
    public int Friendship { get; set; }
    public bool Full { get; set; }
}

