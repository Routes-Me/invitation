using InvitationsService.Abstraction;
using InvitationsService.Models.ResponseModel;
using MessagingLibraries;
using System;
using System.Threading.Tasks;

namespace InvitationsService.Repository
{
    internal class SmsRepository : ISmsRepository
    {
        public async Task SendSMSAsync(InvitationsDto invitationDto, string link)
        {
            try
            {
                if (invitationDto.Address.Substring(0, 3) == "965")
                {
                    var result = await MessagingLibrary.SendSMS(invitationDto.Address, link, 1);
                    if (result.Substring(0, 3) == "ERR")
                        throw new Exception(" Messaging Error : " + result.Split(":")[1]);
                }
                else
                {
                    throw new Exception("ErrorMessage: Provided number doesnot belong to Kuwait.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ErrorMessage: " + ex.Message);
            }
        }
    }
}