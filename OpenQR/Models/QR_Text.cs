using OpenQR.Services;
using System.Drawing;

namespace OpenQR.Models
{
    internal class QR_Text : IQR_CodeData
    {
        public QR_Text(string content)
        {
            Content = content;
            ForegroundColor_Top = "#000";
            ForegroundColor_Bottom = "#000";
            BackgroundColor = "#fff";
        }
        public QR_Text(string content, string fc_t, string fc_b, string b) {
            Content = content;
            ForegroundColor_Top = fc_t;
            ForegroundColor_Bottom = fc_b;
            BackgroundColor = b;
        }
        public QR_Text(string content, string fc_t, string fc_b, string b, Bitmap l, ShapeType s)
        {
            Content = content;
            ForegroundColor_Top = fc_t;
            ForegroundColor_Bottom = fc_b;
            BackgroundColor = b;
            Logo = l;
            ModuleShape = s;
        }

        public string Content { get; set; } = "";
        public string ForegroundColor_Top { get; set; }
        public string ForegroundColor_Bottom { get; set; }
        public string BackgroundColor { get; set; }
        public ShapeType ModuleShape { get; set; } = ShapeType.Square;
        public bool FromLeftToRightCorner { get; set; }
        public Bitmap? Logo { get; set; }
    }
}
