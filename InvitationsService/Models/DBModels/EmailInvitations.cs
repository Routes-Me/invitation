namespace InvitationsService.Models.DBModels
{
    public class EmailInvitations
    {
        public int EmailInvitationId { get; set; }
        public string Email { get; set; }
        public int InvitationId { get; set; }

        public virtual Invitations Invitation { get; set; }
    }
}
