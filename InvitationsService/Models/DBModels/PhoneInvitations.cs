namespace InvitationsService.Models.DBModels
{
    public class PhoneInvitations
    {
        public int PhoneInvitationId { get; set; }
        public string PhoneNumber { get; set; }
        public int InvitationId { get; set; }

        public virtual Invitations Invitation { get; set; }
    }
}