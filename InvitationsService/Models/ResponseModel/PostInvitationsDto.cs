using System.Collections.Generic;

namespace InvitationsService.Models.ResponseModel
{
    public class PostInvitationsDto
    {
        public string OfficerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<int> Roles { get; set; }
        public string InstitutionId { get; set; }
    }
}
