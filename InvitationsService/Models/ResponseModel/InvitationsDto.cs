using System;

namespace InvitationsService.Models.ResponseModel
{
    public class InvitationsDto
    {
        public string InvitationId { get; set; }
        public string RecipientName { get; set; }
        public string ApplicationId { get; set; }
        public string PrivilageId { get; set; }
        public string Address { get; set; }
        public string Method { get; set; }
        public string OfficerId { get; set; }
        public string InstitutionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
