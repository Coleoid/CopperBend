using System.Drawing;
using SadConsole;

namespace CopperBend.Contract
{
    public interface IUIBuilder
    {
        Font MapFont { get; set; }
        FontMaster MapFontMaster { get; set; }
        Size GameSize { get; set; }

        (ControlsConsole, Window) CreateM2Window(Size windowSize, string title);
        (ScrollingConsole, Window) CreateMapWindow(Size windowSize, string title, ICompoundMap fullMap);
        IMessageLogWindow CreateMessageLog();
    }
}
