using System;

namespace InvitationsService.Models.DBModels
{
    public class Invitations
    {
        public int InvitationId { get; set; }
        public string RecipientName { get; set; }
        public int ApplicationId { get; set; }
        public int PrivilageId { get; set; }
        public int OfficerId { get; set; }
        public int InstitutionId { get; set; }
        public InvitationMethods Method { get; set; }
        public UserType? UserType { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual EmailInvitations EmailInvitation { get; set; }
        public virtual PhoneInvitations PhoneInvitation { get; set; }
        public virtual DriverInvitations DriverInvitation { get; set; }


    }
    public enum InvitationMethods
    {
        email,
        phone_number,
        link
    }
    public enum UserType
    {
        user,
        driver
    }
}
