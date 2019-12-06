using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Characters;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Linq;


namespace UpgradedHorse
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        public static bool horseFed = false;
        public static int addedHorseSpeed = 0;

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
        }

        ///// <summary>The method called after a new day starts.</summary>
        ///// <param name="sender">The event sender.</param>
        ///// <param name="e">The event arguments.</param>
        //private void OnDayStarted(object sender, DayStartedEventArgs e)
        //{
        //    //horseFed = true;
        //}

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
        }

        private void OnDayStarted(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady || !Game1.currentLocation.IsFarm)
            {
                return;
            }

            horseFed = false;
            addedHorseSpeed = 0;
        }


        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, EventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            if (Game1.player.isRidingHorse() && addedHorseSpeed == 0 && horseFed)
            {
                addedHorseSpeed = 3;
                Game1.player.addedSpeed += addedHorseSpeed;
            } else if (!Game1.player.isRidingHorse() && addedHorseSpeed > 0)
            {
                Game1.player.addedSpeed -= addedHorseSpeed;
                addedHorseSpeed = 0;
            }
            this.Monitor.Log(string.Format("Speed: {0}", Game1.player.addedSpeed), LogLevel.Debug);
        }


        private void FeedHorse(GameLocation currentLocation, int x, int y)
        {
            if (Game1.player.CurrentItem == null)
            {
                return;
            }

            this.Monitor.Log(string.Format("{0}", Game1.player.CurrentItem.category), LogLevel.Debug);

            // Find if click was on Horse
            foreach (Horse horse in currentLocation.characters.OfType<Horse>())
            {
                if (Utility.withinRadiusOfPlayer((int) (horse.Position.X), (int) (horse.Position.Y), 1, Game1.player)
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
                        } else
                        {
                            Game1.drawObjectDialogue(string.Format("{0} is full.", horse.name));
                        }
                    }
                }
            }
        }

    }

}