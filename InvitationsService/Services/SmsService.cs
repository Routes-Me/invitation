using InvitationsService.Models.ResponseModel;
using MessagingLibraries;
using System;
using System.Threading.Tasks;

namespace InvitationsService.Services
{
    internal class SmsService : ISmsService
    {
        public async Task SendSMSAsync(InvitationsDto invitationDto, string link)
        {
            try
            {
                if (invitationDto.Address.Substring(0, 3) == "965")
                {
                    string result = await MessagingLibrary.SendSMS(invitationDto.Address, link, 1);
                    if (result.Substring(0, 3) == "ERR")
                    {
                        throw new Exception(" Messaging Error : " + result.Split(":")[1]);
                    }
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