using InvitationsService.Abstraction;
using InvitationsService.Models;
using InvitationsService.Models.Common;
using InvitationsService.Models.DBModels;
using InvitationsService.Models.ResponseModel;
using InvitationsService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using RoutesSecurity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace InvitationsService.Repository
{
    public class InvitationsRepository : IInvitationsRepository
    {
        private readonly InvitationsServiceContext _context;
        private readonly AppSettings _appSettings;
        private readonly Dependencies _dependencies;
        private readonly IEmailsRepository _emailRepository;
        private readonly ISmsService _smsService;

        public InvitationsRepository(IOptions<AppSettings> appSettings, IOptions<Dependencies> dependencies, InvitationsServiceContext context, IEmailsRepository emailRepository, ISmsService smsService)
        {
            _appSettings = appSettings.Value;
            _dependencies = dependencies.Value;
            _context = context;
            _emailRepository = emailRepository;
            _smsService = smsService;
        }

        public dynamic DeleteInvitation(string invitationId)
        {
            if (string.IsNullOrEmpty(invitationId))
            {
                throw new ArgumentNullException(CommonMessage.InvalidData);
            }

            Invitations invitation = _context.Invitations.Where(r => r.InvitationId == Obfuscation.Decode(invitationId)).FirstOrDefault();
            if (invitation == null)
            {
                throw new KeyNotFoundException(CommonMessage.InvitationNotFound);
            }

            return invitation;
        }

        public dynamic GetInvitation(string invitationId, Pagination pageInfo)
        {
            List<Invitations> invitations = new List<Invitations>();
            int recordsCount = 1;

            if (!string.IsNullOrEmpty(invitationId))
            {
                invitations = _context.Invitations.Include(i => i.EmailInvitation).Include(i => i.PhoneInvitation).Include(i => i.DriverInvitation).Where(i => i.InvitationId == Obfuscation.Decode(invitationId)).ToList();
            }
            else
            {
                invitations = _context.Invitations.Include(i => i.EmailInvitation).Include(i => i.PhoneInvitation).Include(i => i.DriverInvitation).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();
                recordsCount = _context.Invitations.Count();
            }

            Pagination page = new Pagination
            {
                offset = pageInfo.offset,
                limit = pageInfo.limit,
                total = recordsCount
            };

            dynamic invitationData = invitations.Select(i => new InvitationsDto
            {
                InvitationId = Obfuscation.Encode(i.InvitationId),
                RecipientName = i.RecipientName,
                ApplicationId = Obfuscation.Encode(i.ApplicationId),
                PrivilageId = Obfuscation.Encode(i.PrivilageId),
                OfficerId = Obfuscation.Encode(i.OfficerId),
                InstitutionId = Obfuscation.Encode(i.InstitutionId),
                Method = i.Method.ToString(),
                Address = i.Method == InvitationMethods.email ? i.EmailInvitation.Email : i.PhoneInvitation.PhoneNumber,
                CreatedAt = i.CreatedAt,
                UserType = i.UserType.ToString(),
                VehicleId = i.UserType.ToString() == UserType.driver.ToString() ? Obfuscation.Encode(Convert.ToInt32(i.DriverInvitation.VehicleId)) : ""
            }).ToList();

            return new GetResponse
            {
                data = invitationData,
                pagination = page,
            };
        }


        public async Task<dynamic> PostInvitation(InvitationsDto invitationDto)
        {
            if (invitationDto == null || string.IsNullOrEmpty(invitationDto.OfficerId) || string.IsNullOrEmpty(invitationDto.ApplicationId))
            {
                throw new ArgumentNullException(CommonMessage.InvalidData);
            }

            Invitations invitation = InsertInvitation(invitationDto);

            string url = GetInvitationUrl(invitationDto.ApplicationId, invitation.InvitationId);
            try
            {
                if (invitation.Method == InvitationMethods.email)
                {
                    await _emailRepository.SendEmailAsync(invitationDto, url);
                }
                else if (invitation.Method == InvitationMethods.phone_number)
                {
                    await _smsService.SendSMSAsync(invitationDto, url);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Task.CompletedTask;
        }

        private Invitations InsertInvitation(InvitationsDto invitationDto)
        {
            try
            {
                Invitations invitation = new Invitations();
                invitation.RecipientName = invitationDto.RecipientName;
                invitation.ApplicationId = Obfuscation.Decode(invitationDto.ApplicationId);
                invitation.PrivilageId = Obfuscation.Decode(invitationDto.PrivilageId);
                invitation.OfficerId = Obfuscation.Decode(invitationDto.OfficerId);
                invitation.InstitutionId = Obfuscation.Decode(invitationDto.InstitutionId);
                if (invitationDto.Method == InvitationMethods.email.ToString())
                {
                    invitation.Method = InvitationMethods.email;
                    invitation.EmailInvitation = new EmailInvitations { Email = invitationDto.Address };
                }
                if (invitationDto.Method == InvitationMethods.phone_number.ToString())
                {
                    invitation.Method = InvitationMethods.phone_number;
                    invitation.PhoneInvitation = new PhoneInvitations { PhoneNumber = invitationDto.Address };
                }
                if (invitationDto.Method == InvitationMethods.link.ToString())
                {
                    invitation.Method = InvitationMethods.link;
                    //invitation.LinkInvitation = new LinkInvitations { link = invitationDto.Address };         To be implemented in future
                }

                
                invitation.UserType = invitationDto.UserType == UserType.user.ToString()? UserType.user : UserType.driver;
                invitation.CreatedAt = DateTime.Now;

                _context.Invitations.Add(invitation);
                _context.SaveChanges();

                if (invitation.UserType == UserType.driver && !string.IsNullOrEmpty(invitationDto.VehicleId))
                {
                    DriverInvitations driverInvitations = new DriverInvitations { InvitationId = invitation.InvitationId, VehicleId = Obfuscation.Decode(invitationDto.VehicleId) };
                    _context.DriverInvitations.Add(driverInvitations);
                    _context.SaveChanges();
                }
                return invitation;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetInvitationUrl(string applicationId, int invitationId)
        {
            RegistrationForms registrationForm = _context.RegistrationForms.Where(r => r.ApplicationId == Obfuscation.Decode(applicationId)).FirstOrDefault();
            if (registrationForm == null)
            {
                throw new KeyNotFoundException(CommonMessage.RegistrationFormUrlNotFound);
            }
            string token = JsonConvert.DeserializeObject<InvitationTokenResponse>(GetAPI(_dependencies.GenerateInvitationTokenUrl).Content).invitationToken.ToString();
            if (Obfuscation.Decode(applicationId) == 3)
                return registrationForm.Url + Obfuscation.Encode(invitationId) + "&tk=" + token;
            else
                return registrationForm.Url + "?inv=" + Obfuscation.Encode(invitationId) + "&tk=" + token;
        }

        private dynamic GetAPI(string url, string query = "")
        {
            UriBuilder uriBuilder = new UriBuilder(_appSettings.Host + url);
            uriBuilder = AppendQueryToUrl(uriBuilder, query);
            RestClient client = new RestClient(uriBuilder.Uri);
            RestRequest request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == 0)
            {
                throw new HttpListenerException(400, CommonMessage.ConnectionFailure);
            }

            if (!response.IsSuccessful)
            {
                throw new HttpListenerException((int)response.StatusCode, response.Content);
            }

            return response;
        }

        private UriBuilder AppendQueryToUrl(UriBuilder baseUri, string queryToAppend)
        {
            if (baseUri.Query != null && baseUri.Query.Length > 1)
            {
                baseUri.Query = baseUri.Query.Substring(1) + "&" + queryToAppend;
            }
            else
            {
                baseUri.Query = queryToAppend;
            }
            return baseUri;
        }
    }
}
