using System;
using RLNET;

namespace CopperBend.App
{
    /// <summary>
    /// Function determining how a panel recalculates its size when the window is resized.
    /// </summary>
    /// <returns></returns>
    public delegate int ResizeCalc();

    /// <summary>
    /// Base for rendering on part of the root console
    /// </summary>
    public abstract class Panel
    {
        /// <summary>
        /// Recalculates the X coordinate of the left edge of this panel
        /// </summary>
        protected ResizeCalc rootXCalc;

        /// <summary>
        /// Recalculates the Y coordinate of the top edge of this panel
        /// </summary>
        protected ResizeCalc rootYCalc;

        /// <summary>
        /// Reclaculates the width of this panel
        /// </summary>
        protected ResizeCalc widthCalc;

        /// <summary>
        /// Function that reclaculates the height of the panel.
        /// </summary>
        protected ResizeCalc heightCalc;

        /// <summary>
        /// The offscreen console which the panel will update to.
        /// </summary>
        protected RLConsole console;

        /// <summary>
        /// The X-coordinate of the top left corner of the section of the root console that this panel will begin rendering to.
        /// </summary>
        public int RootX { get; private set; }

        /// <summary>
        /// The Y-coordinate of the top left corner of the section of the root console that this panel will begin rendering to.
        /// </summary>
        public int RootY { get; private set; }

        public bool Visible { get; private set; }

        /// <summary>
        /// Whether or not the panel should run the code to update its offscreen console every frame. Things like menus, that don't change
        /// often if at all after initial rendering (outside of special cases like resizing), should not be real time, while something like a map
        /// camera panel should likely be real time.
        /// </summary>
        public bool RealTimeUpdate { get; protected set; }

        private bool _acceptsKeyboardInput;
        public bool AcceptsKeyboardInput
        {
            get => _acceptsKeyboardInput;
            set
            {
                throw new Exception("Need to reimplement this static global approach");
                //if (_acceptsKeyboardInput != value)
                //{
                //    _acceptsKeyboardInput = value;
                //    if (Shown)
                //    {
                //        if (_acceptsKeyboardInput)
                //            Engine.AddToKeyPressFront(OnKeyPress);
                //        else
                //            Engine.KeyPress -= OnKeyPress;
                //    }
                //}
            }
        }

        /// <summary>
        /// The width of the section of the root console that this panel is rendering to.
        /// </summary>
        public int Width
        {
            get => console.Width;
        }

        /// <summary>
        /// The height of the section of the root console that this panel is rendering to.
        /// </summary>
        public int Height
        {
            get => console.Height;
        }

        /// <summary>
        /// Called just before the console resizes.
        /// </summary>
        public event EventHandler<ResizeEventArgs> OnResizing;

        /// <summary>
        /// Called after console resizes but before it updates layout.
        /// </summary>
        public event EventHandler OnResize;

        /// <summary>
        /// Constructs a panel.
        /// </summary>
        /// <param name="rootX">A function that takes no parameters and returns an int, saying where the X-coordinate of the top left corner of the section of the
        /// root console that the panel will render to should be.</param>
        /// <param name="rootY">A function that takes no parameters and returns an int, saying where the Y-coordinate of the top left corner of the section of the
        /// root console that the panel will render to should be.</param>
        /// <param name="width">A function that takes no parameters and returns an int, saying what the width of the section of the
        /// root console that the panel will render to should be.</param>
        /// <param name="height">A function that takes no parameters and returns an int, saying what the height of the section of the
        /// root console that the panel will render to should be.</param>
        /// <param name="acceptsKeyboardInput">Whether or not the panel will accept keyboard input (have its OnKeyPress function called) when it is
        /// being shown.</param>
        /// <param name="realTimeUpdate">Whether or not the panel should run the code to update its offscreen console every frame.</param>
        protected Panel(ResizeCalc rootX, ResizeCalc rootY, ResizeCalc width, ResizeCalc height, bool acceptsKeyboardInput = false, bool realTimeUpdate = false)
        {
            rootXCalc = rootX;
            rootYCalc = rootY;
            widthCalc = width;
            heightCalc = height;

            RootX = rootXCalc();
            RootY = rootYCalc();

            console = new RLConsole(widthCalc(), heightCalc());
            Visible = false;
            RealTimeUpdate = realTimeUpdate;
            _acceptsKeyboardInput = acceptsKeyboardInput;
            OnResize = null;
        }

        /// <summary>
        /// Implement to update the panel's offscreen console.  A menu would draw all the text in this function, while a map camera might draw a section of the map.
        /// If RealTimeUpdate is true, this is called every frame before any panels start having their consoles blitted to the root console.  If RealTimeUpdate is false,
        /// this is called when the panel is shown, and when it is resized, in addition to any times it might be called manually.  The UpdateRealTimeLayouts event
        /// in Program is what triggers this in the case of real-time updating.  The z-level things are printed on ONLY has an effect within this panel.  Between panels,
        /// the last one shown gets drawn on top regardless of the z-level their text is drawn on.
        /// </summary>
        /// <param name="sender">The object sending the event, in the case RealTimeUpdate is true.  If RealTimeUpdate is false, this is a reference to the panel
        /// object it is a part of (this pointer).</param>
        /// <param name="e">The arguments sent by the event, in the case RealTimeUpdate is true.  If RealTimeUpdate is false, the UpdateEventArgs has a time value
        /// of 0.0.</param>
        abstract public void UpdateLayout(object sender, UpdateEventArgs e);

        /// <summary>
        /// Called every frame (if the panel is shown) automatically, via the Render event of Program.  Simply blits the panel's offscreen console to the appropriate
        /// section of the root console.
        /// </summary>
        /// <param name="sender">The object sending the Render event.</param>
        /// <param name="e">Contains the delta time between the last render frame and this one.</param>
        virtual public void Render(object sender, UpdateEventArgs e)
        {
            RLConsole.Blit(console, 0, 0, Width, Height, GameLoop.RootConsole, RootX, RootY);
        }

        /// <summary>
        /// Should be implemented to handle keys apprpriately as the panel requires.  Default implementation does nothing.
        /// </summary>
        /// <param name="sender">Sender of key press event.</param>
        /// <param name="e">KeyPress arguments including the key pressed, etc.</param>
        protected virtual void OnKeyPress(object sender, EventArgs_KeyPress e) { }

        /// <summary>
        /// Causes the panel to be rendered (shown to the program user) each render frame, and to accept keyboard input if AcceptsKeyboardInput is true,
        /// if it is not already shown.  If RealTimeUpdate is true, the panel's UpdateLayout handler is added to the Program.UpdateRealTimeLayout event
        /// handler list.  If RealTimeUpdate is false, it simply calls UpdateLayout once.
        /// </summary>
        public void Show()
        {
            if (Visible) throw new Exception("Showing panel that is visible");

            if (RealTimeUpdate)
                GameLoop.UpdateRealTimeLayouts += UpdateLayout;
            else
                UpdateLayout(this, new UpdateEventArgs(0f));

            GameLoop.Render += Render;

            if (_acceptsKeyboardInput)
                GameLoop.AddToKeyPressFront(OnKeyPress);

            Visible = true;
        }

        /// <summary>
        /// Causes the panel to stop being rendered (shown to the user) and to stop receiving keyboard input, if it is currently shown.
        /// If RealTimeUpdate is true, the panel's UpdateLayout handler is removed from the Program.UpdateRealTimeLayout event handler list.
        /// </summary>
        public void Hide()
        {
            if (!Visible) throw new Exception("Hiding a hidden panel");

            GameLoop.Render -= Render;

            if (_acceptsKeyboardInput)
                GameLoop.KeyPress -= OnKeyPress;

            if (RealTimeUpdate)
                GameLoop.UpdateRealTimeLayouts -= UpdateLayout;

            Visible = false;
        }


#pragma warning disable RECS0154

        // Called internally to resize all panels from screen
        internal void onScreenResize(object sender, ResizeEventArgs e)
#pragma warning restore RECS0154
        {
            OnResizing?.Invoke(this, e);

            RootX = rootXCalc();
            RootY = rootYCalc();
            console.Resize(widthCalc(), heightCalc());

            OnResize?.Invoke(this, EventArgs.Empty);

            if (!RealTimeUpdate)
                UpdateLayout(this, null);
        }
    }
}
