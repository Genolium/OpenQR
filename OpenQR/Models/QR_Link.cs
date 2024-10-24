using System.Drawing;
using OpenQR.Services;

namespace OpenQR.Models
{  
    internal class QR_Link : IQR_CodeData
    {
        public QR_Link(string url)
        {
            ForegroundColor_Top = "#000";
            ForegroundColor_Bottom = "#000";
            BackgroundColor = "#fff";

            Content = url;
        }

        public QR_Link(string url, string fc_t, string fc_b, string b)
        {
            ForegroundColor_Top = fc_t;
            ForegroundColor_Bottom = fc_b;
            BackgroundColor = b;

            Content = url;
        }

        public QR_Link(string url, string fc_t, string fc_b, string b, Bitmap l)
        {
            ForegroundColor_Top = fc_t;
            ForegroundColor_Bottom = fc_b;
            BackgroundColor = b;
            Logo = l;

            Content = url;
        }

        public QR_Link(string url, string fc_t, string fc_b, string b, Bitmap l, string p, ShapeType s)
        {
            ForegroundColor_Top = fc_t;
            ForegroundColor_Bottom = fc_b;
            BackgroundColor = b;
            Logo = l;
            Protocol = p;
            Content = url;
            ModuleShape = s;
        }

        public string Content { get; set; }
        public string ForegroundColor_Top { get; set; }
        public string ForegroundColor_Bottom { get; set; }
        public string BackgroundColor { get; set; }
        public ShapeType ModuleShape { get; set; }
        public string Protocol { get; private set; } = "http://";
        public bool FromLeftToRightCorner { get; set; }
        public Bitmap? Logo { get; set; }
    }
}
