using System;

namespace InvitationsService.Models.DBModels
{
    public class RegistrationForms
    {
        public int RegistrationFormId { get; set; }
        public int ApplicationId { get; set; }
        public string Url { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
