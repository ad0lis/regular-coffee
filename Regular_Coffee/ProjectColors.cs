namespace Regular_Coffee
{
    using System;
    using System.Drawing;

    public static class ProjectColors
    {
        private static Color _projBlue = Color.FromArgb(int.Parse("FF088AA8", System.Globalization.NumberStyles.HexNumber));
        private static Color _projTeal = Color.FromArgb(int.Parse("FF00667E", System.Globalization.NumberStyles.HexNumber));
        private static Color _projDarkTeal = Color.FromArgb(int.Parse("FF00566A", System.Globalization.NumberStyles.HexNumber));
        private static Color _projOrange = Color.FromArgb(int.Parse("FFA83808", System.Globalization.NumberStyles.HexNumber));

        public static Color DarkTeal => _projDarkTeal;

        public static Color Blue => _projBlue;

        public static Color Orange => _projOrange;

        public static Color Teal => _projTeal;
    }
}