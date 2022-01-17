using InvitationsService.Models.ResponseModel;
using System.Threading.Tasks;

namespace InvitationsService.Abstraction
{
    public interface ISmsRepository
    {
        Task SendSMSAsync(InvitationsDto invitationDto, string link);
    }
}
