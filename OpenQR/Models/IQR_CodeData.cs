using OpenQR.Services;
using System.Drawing;

namespace OpenQR.Models
{ 
    public interface IQR_CodeData
    {   
        public string ForegroundColor_Top { get; set; }
        public string ForegroundColor_Bottom { get; set; }
        public string BackgroundColor { get; set; }
        ShapeType ModuleShape { get; set; }
        public Bitmap? Logo { get; set; }
        public string Content { get; set; }
    }

    public enum EncodingType
    {
        ТЕКСТ,
        ССЫЛКА,
        ТЕЛЕФОН,
        ПОЧТА,
        WhatsApp
    }
}
