using System.Threading.Tasks;
using InvitationsService.Models.Common;
using InvitationsService.Models.ResponseModel;
using static MessagingLibraries.MessagingLibrary;

namespace InvitationsService.Services
{
    public interface ISenderService
    {
        Task<string> SendVerificationLink(InvitationsDto invitationDto, string link);
    }
    public class SenderService : ISenderService
    {
        public async Task<string> SendVerificationLink(InvitationsDto invitationDto, string link)
        {
            // if(invitationDto.Method == InvitationConfig.SMSChannel)
            return await SendSMS("verify.PhoneNumber", "otp", 1);

            // if(invitationDto.Method == InvitationConfig.EmailChannel)
            // return await 
        }
    }
}