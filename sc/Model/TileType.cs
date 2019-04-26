using Microsoft.Xna.Framework;

namespace CopperBend.Model
{
    public class TileType
    {
        public string Name { get; set; }
        public char Symbol { get; set; }
        public bool IsWalkable { get; set; }
        public bool IsTransparent { get; set; }
        public bool IsTillable { get; internal set; }

        internal void SetForeground(Color colorUnseen, Color colorSeen)
        {
            _fgUnseen = colorUnseen;
            _fgSeen = colorSeen;
        }
        public Color Foreground(bool isInFOV) => isInFOV ? _fgSeen : _fgUnseen;
        private Color _fgSeen;
        private Color _fgUnseen;

        internal void SetBackground(Color colorUnseen, Color colorSeen)
        {
            _bgUnseen = colorUnseen;
            _bgSeen = colorSeen;
        }
        public Color Background(bool isInFOV) => isInFOV ? _bgSeen : _bgUnseen;
        private Color _bgSeen = Color.LightGray;
        private Color _bgUnseen = Color.DarkGray;
    }
}
