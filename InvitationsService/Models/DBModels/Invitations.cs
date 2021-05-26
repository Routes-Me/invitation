using System;

namespace InvitationsService.Models.DBModels
{
    public class Invitations
    {
        public int InvitationId { get; set; }
        public string RecipientName { get; set; }
        public int ApplicationId { get; set; }
        public string Address { get; set; }
        public byte[] Data { get; set; }
        public int OfficerId { get; set; }
        public int InstitutionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
