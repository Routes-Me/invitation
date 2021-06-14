using System.Collections.Generic;
using System.Threading.Tasks;
using InvitationsService.Models.ResponseModel;

namespace InvitationsService.Abstraction
{
    public interface IEmailsRepository
    {
        Task SendEmailAsync(InvitationsDto invitationDto, string link);
    }
}
