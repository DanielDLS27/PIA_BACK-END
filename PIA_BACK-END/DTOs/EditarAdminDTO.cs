using System.ComponentModel.DataAnnotations;

namespace PIA_BACK_END.DTOs
{
    public class EditarAdminDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
