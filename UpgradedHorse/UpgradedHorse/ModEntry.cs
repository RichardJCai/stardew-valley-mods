using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Characters;
using StardewValley;
using System.Linq;
using StardewValley.Menus;
using System.Collections.Generic;
using StardewValley.Buildings;
using StardewValley.Locations;


namespace UpgradedHorseMod
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        public static bool horseFed = false;
        public static int addedHorseSpeed = 0;

        public static int openMenuX;
        public static int openMenuY;

        public const int INVENTORY_TAB = 0;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
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

            if (e.Button == SButton.MouseLeft)
            {
                Point cursorPosition = Game1.getMousePosition();
                OpenHorseMenu(cursorPosition.X,
                          cursorPosition.Y);
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
        private void OnUpdateTicking(object sender, EventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            if (Game1.player.isRidingHorse() && addedHorseSpeed == 0 && horseFed)
            {
                addedHorseSpeed = 3;
                Game1.player.addedSpeed += addedHorseSpeed;
            }
            else if (!Game1.player.isRidingHorse() && addedHorseSpeed > 0)
            {
                Game1.player.addedSpeed -= addedHorseSpeed;
                addedHorseSpeed = 0;
            }
            this.Monitor.Log(string.Format("Speed: {0}", Game1.player.addedSpeed), LogLevel.Debug); 
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs args)
        {
            if (args.NewMenu is GameMenu gm)
            {
                openMenuX = gm.xPositionOnScreen;
                openMenuY = gm.yPositionOnScreen;

                //Helper.Events.Display.RenderedActiveMenu += drawTest;
                //Utility.drawWithShadow(b, Game1.mouseCursors,
                //    new Vector2((float)(
                //    (double)(gm.xPositionOnScreen + Game1.tileSize * 5 + Game1.pixelZoom * 2) + (double)Math.Max((float)Game1.tileSize, Game1.dialogueFont.MeasureString(Game1.player.name).X / 2f) + (Game1.player.getPetDisplayName() != null ? (double)Math.Max((float)Game1.tileSize, Game1.dialogueFont.MeasureString(Game1.player.getPetDisplayName()).X) : 0.0)), (float)(gm.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 7 * Game1.tileSize - Game1.pixelZoom)), new Rectangle(193, 192, 16, 16), Color.White, 0.0f, Vector2.Zero,
                //    (float)Game1.pixelZoom, false, -1f, -1, -1, 0.35f);
            }
            else if (args.OldMenu is GameMenu ogm)
            {
                openMenuX = ogm.xPositionOnScreen;
                openMenuY = ogm.yPositionOnScreen;
            }

            this.Monitor.Log(string.Format("{0}, {1}", openMenuX, openMenuY), LogLevel.Debug);

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

        private void OpenHorseMenu(int x, int y)
        {
            if (Game1.activeClickableMenu is GameMenu menu)
            {
                this.Monitor.Log(string.Format("{0}", menu.currentTab), LogLevel.Debug);
                if (menu.currentTab == INVENTORY_TAB)
                {
                    Vector2 rectangle = new Vector2((float)((double)(openMenuX + Game1.tileSize * 5 + Game1.pixelZoom * 2) + (double)Math.Max((float)Game1.tileSize, Game1.dialogueFont.MeasureString(Game1.player.name).X / 2f) + (Game1.player.getPetDisplayName() != null ? (double)Math.Max((float)Game1.tileSize, Game1.dialogueFont.MeasureString(Game1.player.getPetDisplayName()).X) : 0.0)), (float)(openMenuY + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 7 * Game1.tileSize - Game1.pixelZoom));
                    this.Monitor.Log(string.Format("{0}, {1}, {2}, {3}", rectangle.X, x, rectangle.Y, y), LogLevel.Debug);

                    if (Utility.distance((float)x, rectangle.X, (float)y, rectangle.Y) <= 100)
                    {
                        String horseName = Game1.player.horseName;
                        this.Monitor.Log(
                            string.Format("Horse name: {0}, {1}", horseName, "test")
                            );

                        HorseData horseData = this.Helper.Data.ReadGlobalData<HorseData>(
                            String.Format("{0}-horse-data", Game1.player.Name) // Not sure if player name is unique
                            );

                        if (horseData == null)
                        {
                            this.Monitor.Log(
                            string.Format("nil")
                            );
                            horseData = new HorseData(0, 0, 0);
                        }
                        UpgradedHorse horse = new UpgradedHorse(
                            horseName,
                            horseData.Friendship,
                            horseData.Fullness,
                            horseData.Happiness);
                        Game1.activeClickableMenu = (IClickableMenu)new HorseMenu(horse);
                    }
                }
            }
        }

        /// <summary>Get all available locations.</summary>
        private IEnumerable<GameLocation> GetLocations()
        {
            GameLocation[] mainLocations = (Context.IsMainPlayer ? Game1.locations : this.Helper.Multiplayer.GetActiveLocations()).ToArray();

            foreach (GameLocation location in mainLocations.Concat(MineShaft.activeMines))
            {
                yield return location;

                if (location is BuildableGameLocation buildableLocation)
                {
                    foreach (Building building in buildableLocation.buildings)
                    {
                        if (building.indoors.Value != null)
                            yield return building.indoors.Value;
                    }
                }
            }
        }

        /// <summary>Find the current player's horse.</summary>
        private Horse FindHorse()
        {
            foreach (NPC npc in Utility.getAllCharacters())
            {
                if (npc is Horse)
                {
                    Horse horse = (Horse)npc;
                    if (horse.getOwner() == Game1.player) {
                        return horse;
                    }
                }
            }
            //foreach (GameLocation location in this.GetLocations())
            //{
            //    foreach (Horse horse in location.characters.OfType<Horse>())
            //    {
            //        if (horse.rider != null)
            //            continue;

            //        if (horse.getOwner() == Game1.player)
            //            return horse;
            //    }
            //}

            return null;
        }

    }

}

class HorseData
{
    public HorseData(double friendship, double fullness, double happiness)
    {
        Friendship = friendship;
        Fullness = fullness;
        Happiness = happiness;
    }
    public double Friendship { get; set; }
    public double Fullness { get; set; }
    public double Happiness { get; set; }
}
