using Newtonsoft.Json.Linq;
using InvitationsService.Models.ResponseModel;
using System.Collections.Generic;

namespace InvitationsService.Models
{
    public class ErrorResponse
    {
        public string error { get; set; }
    }
    public class SuccessResponse
    {
        public string message { get; set; }
    }
    public class GetResponse
    {
        public Pagination pagination { get; set; }
        public List<InvitationsDto> data { get; set; }
    }
    public class InvitationTokenResponse
    {
        public string invitationToken { get; set; }
    }
}
