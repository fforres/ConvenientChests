using System;
using ConvenientChests.StashToChests;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Objects;

namespace ConvenientChests.CategorizeChests.Interface.Widgets {
    internal class ChestOverlay : Widget {

        private readonly InventoryMenu inventoryMenu;

        private readonly InventoryMenu.highlightThisItem defaultChestHighlighter;

        private readonly InventoryMenu.highlightThisItem defaultInventoryHighlighter;

        private TextButton OpenButton {
            get; set;
        }

        private TextButton StashButton {
            get; set;
        }

        private CategoryMenu CategoryMenu {
            get; set;
        }

        private ItemGrabMenu ItemGrabMenu {
            get;
        }

        private CategorizeChestsModule Module {
            get;
        }

        private Chest Chest {
            get;
        }

        private ITooltipManager TooltipManager {
            get;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Revised")]
        public ChestOverlay(CategorizeChestsModule module, Chest chest, ItemGrabMenu menu, ITooltipManager tooltipManager) {
            this.Module = module;
            this.Chest = chest;
            this.ItemGrabMenu = menu;
            this.inventoryMenu = menu.ItemsToGrabMenu;
            this.TooltipManager = tooltipManager;

            this.defaultChestHighlighter = this.ItemGrabMenu.inventory.highlightMethod;
            this.defaultInventoryHighlighter = this.inventoryMenu.highlightMethod;

            this.AddButtons();
        }

        public override bool ReceiveLeftClick(Point point) {
            var hit = this.PropagateLeftClick(point);
            if (!hit && this.CategoryMenu != null) {
                // Are they clicking outside the menu to try to exit it?
                this.CloseCategoryMenu();
            }

            return hit;
        }

        protected override void OnParent(Widget parent) {
            base.OnParent(parent);

            if (parent == null) {
                return;
            }

            this.Width = parent.Width;
            this.Height = parent.Height;
        }

        private void AddButtons() {
            this.OpenButton = new TextButton("Categorize", Sprites.LeftProtrudingTab);
            this.OpenButton.OnPress += this.ToggleMenu;
            this.AddChild(this.OpenButton);

            this.StashButton = new TextButton(this.ChooseStashButtonLabel(), Sprites.LeftProtrudingTab);
            this.StashButton.OnPress += this.StashItems;
            this.AddChild(this.StashButton);

            this.PositionButtons();
        }

        private void PositionButtons() {
            this.StashButton.Width = this.OpenButton.Width = Math.Max(this.StashButton.Width, this.OpenButton.Width);

            this.OpenButton.Position = new Point(
                this.ItemGrabMenu.xPositionOnScreen + (this.ItemGrabMenu.width / 2) - this.OpenButton.Width - (112 * Game1.pixelZoom),
                this.ItemGrabMenu.yPositionOnScreen + (22 * Game1.pixelZoom)
            );

            this.StashButton.Position = new Point(
                this.OpenButton.Position.X + this.OpenButton.Width - this.StashButton.Width,
                this.OpenButton.Position.Y + this.OpenButton.Height - 0
            );
        }

        private string ChooseStashButtonLabel() {
            return this.Module.Config.StashKey == SButton.None
                       ? "Stash"
                       : $"Stash ({this.Module.Config.StashKey})";
        }

        private void ToggleMenu() {
            if (this.CategoryMenu == null) {
                this.OpenCategoryMenu();
            } else {
                this.CloseCategoryMenu();
            }
        }

        private void OpenCategoryMenu() {
            var chestData = this.Module.ChestDataManager.GetChestData(this.Chest);
            this.CategoryMenu = new CategoryMenu(chestData, this.Module.ItemDataManager, this.TooltipManager, this.ItemGrabMenu.width - 24);
            this.CategoryMenu.Position = new Point(
                    this.ItemGrabMenu.xPositionOnScreen - this.GlobalBounds.X - 12,
                    this.ItemGrabMenu.yPositionOnScreen - this.GlobalBounds.Y - 60
                );

            this.CategoryMenu.OnClose += this.CloseCategoryMenu;
            this.AddChild(this.CategoryMenu);

            this.SetItemsClickable(false);
        }

        private void CloseCategoryMenu() {
            this.RemoveChild(this.CategoryMenu);
            this.CategoryMenu = null;

            this.SetItemsClickable(true);
        }

        private void StashItems() => StackLogic.StashToChest(this.Chest, ModEntry.StashNearby.AcceptingFunction);

        private void SetItemsClickable(bool clickable) {
            if (clickable) {
                this.ItemGrabMenu.inventory.highlightMethod = this.defaultChestHighlighter;
                this.inventoryMenu.highlightMethod = this.defaultInventoryHighlighter;
            }
            else {
                this.ItemGrabMenu.inventory.highlightMethod = i => false;
                this.inventoryMenu.highlightMethod = i => false;
            }
        }
    }
}