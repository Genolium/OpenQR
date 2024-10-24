using System.ComponentModel.DataAnnotations;

namespace OpenQR.Models
{
    public enum EncodingType
    {
        [Display(Name = "ТЕКСТ")]
        TEXT,
        [Display(Name = "ССЫЛКА")]
        LINK,
        [Display(Name = "ТЕЛЕФОН")]
        PHONE,
        [Display(Name = "ПОЧТА")]
        EMAIL,
        [Display(Name = "WhatsApp")]
        WHATSAPP
    }
}
