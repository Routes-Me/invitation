using InvitationsService.Abstraction;
using InvitationsService.Models.ResponseModel;
using System.Threading.Tasks;
using MessagingLibraries;
using System;

namespace InvitationsService.Repository
{
    internal class SmsRepository : ISmsRepository
    {
        public async Task SendSMSAsync(InvitationsDto invitationDto, string link)
        {
            try
            {
                await MessagingLibrary.SendSMS(invitationDto.Address, link, 1);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}