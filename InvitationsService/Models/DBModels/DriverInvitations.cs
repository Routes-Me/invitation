namespace InvitationsService.Models.DBModels
{
    public class DriverInvitations
    {
        public int DriverInvitationsId { get; set; }
        public int? VehicleId { get; set; }
        public int InvitationId { get; set; }

        public virtual Invitations Invitations { get; set; }
    }
}
