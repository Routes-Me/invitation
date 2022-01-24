using InvitationsService.Models.ResponseModel;
using System.Threading.Tasks;

namespace InvitationsService.Services
{
    public interface ISmsService
    {
        Task SendSMSAsync(InvitationsDto invitationDto, string link);
    }
}
