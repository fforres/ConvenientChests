using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace ConvenientChests.CategorizeChests.Interface.Widgets
{
    /// <summary>
    /// A positioned, resizable element in the interface
    /// that can also contain other elements.
    /// </summary>
    public class Widget : IDisposable
    {
        private Widget privateParent;

        private int privateWidth;

        private int privateHeight;

        private List<Widget> childrenList = new List<Widget>();

        public Widget Parent {
            get => this.privateParent;
            set
            {
                this.privateParent = value;
                this.OnParent(value);
            }
        }


        public IEnumerable<Widget> Children => this.childrenList;

        public Point Position { get; set; }

        public int X {
            get => this.Position.X;
            set {
                this.Position = new Point(value, this.Position.Y);
            }
        }

        public int Y {
            get => this.Position.Y;
            set {
                this.Position = new Point(this.Position.X, value);
            }
        }


        public int Width
        {
            get => this.privateWidth;
            set
            {
                this.privateWidth = value;
                this.OnDimensionsChanged();
            }
        }

        public int Height
        {
            get { return this.privateHeight; }
            set
            {
                this.privateHeight = value;
                this.OnDimensionsChanged();
            }
        }

        public Widget()
        {
            Position = Point.Zero;
            Width = 1;
            Height = 1;
        }

        protected virtual void OnParent(Widget parent)
        {
        }

        public virtual void Draw(SpriteBatch batch)
        {
            DrawChildren(batch);
        }

        protected void DrawChildren(SpriteBatch batch)
        {
            foreach (var child in Children)
            {
                child.Draw(batch);
            }
        }

        public Rectangle LocalBounds => new Rectangle(0, 0, Width, Height);
        public Rectangle GlobalBounds => new Rectangle(GlobalPosition.X, GlobalPosition.Y, Width, Height);
        public Point GlobalPosition => Globalize(Point.Zero);

        public bool Contains(Point point)
        {
            return point.X >= Position.X && point.X <= Position.X + Width
                   && point.Y >= Position.Y && point.Y <= Position.Y + Height;
        }

        public Point Globalize(Point point)
        {
            var global = new Point(point.X + Position.X, point.Y + Position.Y);
            return Parent != null ? Parent.Globalize(global) : global;
        }

        public virtual bool ReceiveButtonPress(SButton input)
        {
            return PropagateButtonPress(input);
        }

        public virtual bool ReceiveLeftClick(Point point)
        {
            return PropagateLeftClick(point);
        }

        public virtual bool ReceiveCursorHover(Point point)
        {
            return PropagateCursorHover(point);
        }

        public virtual bool ReceiveScrollWheelAction(int amount)
        {
            return PropagateScrollWheelAction(amount);
        }

        protected bool PropagateButtonPress(SButton input)
        {
            foreach (var child in Children)
            {
                var handled = child.ReceiveButtonPress(input);
                if (handled)
                    return true;
            }

            return false;
        }

        protected bool PropagateScrollWheelAction(int amount)
        {
            foreach (var child in Children)
            {
                var handled = child.ReceiveScrollWheelAction(amount);
                if (handled)
                    return true;
            }

            return false;
        }

        protected bool PropagateLeftClick(Point point)
        {
            foreach (var child in Children)
            {
                var localPoint = new Point(point.X - child.Position.X, point.Y - child.Position.Y);

                if (child.LocalBounds.Contains(localPoint))
                {
                    var handled = child.ReceiveLeftClick(localPoint);
                    if (handled)
                        return true;
                }
            }
            return false;
        }

        protected bool PropagateCursorHover(Point point)
        {
            foreach (var child in Children)
            {
                var localPoint = new Point(point.X - child.Position.X, point.Y - child.Position.Y);

                if (child.LocalBounds.Contains(localPoint))
                {
                    var handled = child.ReceiveCursorHover(localPoint);
                    if (handled)
                        return true;
                }
            }
            return false;
        }

        public T AddChild<T>(T child) where T : Widget
        {
            child.Parent = this;
            this.childrenList.Add(child);

            OnContentsChanged();

            return child;
        }

        public void RemoveChild(Widget child)
        {
            this.childrenList.Remove(child);
            child.Parent = null;

            OnContentsChanged();
        }

        public void RemoveChildren()
        {
            RemoveChildren(c => true);
        }

        public void RemoveChildren(Predicate<Widget> shouldRemove)
        {
            foreach (var child in Children.Where(c => shouldRemove(c)))
            {
                child.Parent = null;
            }

            this.childrenList.RemoveAll(shouldRemove);

            OnContentsChanged();
        }

        protected virtual void OnContentsChanged()
        {
        }

        protected virtual void OnDimensionsChanged()
        {
        }

        public void CenterHorizontally()
        {
            var containerWidth = (Parent != null) ? Parent.Width : Game1.viewport.Width; // TODO
            X = containerWidth / 2 - Width / 2;
        }

        public void CenterVertically()
        {
            var containerHeight = (Parent != null) ? Parent.Height : Game1.viewport.Height; // TODO
            Y = containerHeight / 2 - Height / 2;
        }

        public virtual void Dispose() {
            foreach (Widget child in Children)
                child.Dispose();
        }
    }
}