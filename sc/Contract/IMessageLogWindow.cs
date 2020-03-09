using System;

namespace CopperBend.Contract
{
    public interface IMessageLogWindow
    {
        void Draw(TimeSpan drawTime);
        void Update(TimeSpan time);

        /// <summary> add a complete line to the messages </summary>
        void WriteLine(string message);

        /// <summary> add an unfinished line to the messages </summary>
        void Prompt(string message);

        void Show();
        //void Hide();
        //void Center();
        //void Print(int x, int y, string text);
        //void Print(int x, int y, string text, Color foreground);
        //void Print(int x, int y, string text, Color foreground, Color background);
        //void Print(int x, int y, string text, Color foreground, Color background, SpriteEffects mirror);
        //void Print(int x, int y, string text, SpriteEffects mirror);
        //void Print(int x, int y, string text, Cell appearance, ICellEffect effect);
        //int Width { get; }
        //int Height { get; }
        //Cell[] Cells { get; }
    }
}
