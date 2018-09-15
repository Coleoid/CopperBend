using System;
using RLNET;

namespace CopperBend.App
{

    public static class GameLoop
    {
        public static RLRootConsole RootConsole { get; private set; } = null;

        /// <summary>
        /// Any event handlers that are added to this event are called
        /// BEFORE any offscreen consoles are blitted to the root console,
        /// but AFTER any Update() handlers.  Used by panels that need to 
        /// update their offscreen consoles (clear and reset them) in 
        /// real-time (every time a render frame happens)
        /// NOTE: the actual RENDERING is real-time regardless.  Every frame, 
        /// the ROOT console is cleared, and all offscreen consoles from panels
        /// are blitted to it.
        /// </summary>
        public static event UpdateEventHandler UpdateRealTimeLayouts = null;

        /// <summary>
        /// This is the event that is designed to call every panel's render 
        /// function each frame.  Panels automatically add their render functions 
        /// to this handler when they are shown, there is no need to do it manually.
        /// </summary>
        public static event UpdateEventHandler Render = null;

        /// <summary>
        /// To be used by panels to get keyboard events.  It is cancelable.
        /// </summary>
        public static event EventHandler<EventArgs_KeyPress> KeyPress = null;

        /// <summary>
        /// Whether or not the window is fullscreen.
        /// </summary>
        public static bool Fullscreen { get; private set; }

        /// <summary>
        /// Starts an RLRootConsole
        /// </summary>
        /// <param name="settings">Settings passed to root console to initialize.</param>
        public static void Init(RLSettings settings)
        {
            if (RootConsole != null)
                throw new Exception("Init already called!");

            Fullscreen = (settings.StartWindowState == RLWindowState.Fullscreen);

            RootConsole = new RLRootConsole(settings);
            RootConsole.Update += onUpdate;
            RootConsole.Render += onRender;
        }

        public static void Run()
        {
            RootConsole.Run();
        }

        public static void ToggleFullscreen()
        {
            Fullscreen = !Fullscreen;
            RootConsole.SetWindowState(Fullscreen ? RLWindowState.Fullscreen : RLWindowState.Normal);
        }


        private static void onRender(object sender, UpdateEventArgs e)
        {
            // Tell all panels that want to update in real time to do so
            UpdateRealTimeLayouts?.Invoke(sender, e);

            // Draw all shown screens
            RootConsole.Clear();
            Render?.Invoke(sender, e);

            RootConsole.Draw();
        }

        private static void onUpdate(object sender, UpdateEventArgs e)
        {
            if (KeyPress == null) return;
            RLKeyPress key = RootConsole.Keyboard.GetKeyPress();
            if (key == null) return;

            EventArgs_KeyPress args = new EventArgs_KeyPress(key);

            foreach (EventHandler<EventArgs_KeyPress> handler in KeyPress.GetInvocationList())
            {
                handler(sender, args);
                if (args.Cancel) break;
            }
        }

        internal static void AddToKeyPressFront(EventHandler<EventArgs_KeyPress> handler) => KeyPress = handler + KeyPress;
    }
}
