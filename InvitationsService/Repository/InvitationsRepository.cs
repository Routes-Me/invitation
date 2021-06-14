using InvitationsService.Abstraction;
using InvitationsService.Models;
using InvitationsService.Models.DBModels;
using InvitationsService.Models.ResponseModel;
using InvitationsService.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RoutesSecurity;
using RestSharp;

namespace InvitationsService.Repository
{
    public class InvitationsRepository : IInvitationsRepository
    {
        private readonly InvitationsServiceContext _context;
        private readonly AppSettings _appSettings;
        private readonly Dependencies _dependencies;
        private readonly IEmailsRepository _emailRepository;

        public InvitationsRepository(IOptions<AppSettings> appSettings, IOptions<Dependencies> dependencies, InvitationsServiceContext context, IEmailsRepository emailRepository)
        {
            _appSettings = appSettings.Value;
            _dependencies = dependencies.Value;
            _context = context;
            _emailRepository = emailRepository;
        }

        public dynamic DeleteInvitation(string invitationId)
        {
            if (string.IsNullOrEmpty(invitationId))
                throw new ArgumentNullException(CommonMessage.InvalidData);

            Invitations invitation = _context.Invitations.Where(r => r.InvitationId == Obfuscation.Decode(invitationId)).FirstOrDefault();
            if (invitation == null)
                throw new KeyNotFoundException(CommonMessage.InvitationNotFound);

            return invitation;
        }

        public dynamic GetInvitation(string invitationId, Pagination pageInfo)
        {
            List<Invitations> invitations = new List<Invitations>();
            int recordsCount = 1;

            if (!string.IsNullOrEmpty(invitationId))
                invitations = _context.Invitations.Include(i => i.EmailInvitation).Where(i => i.InvitationId == Obfuscation.Decode(invitationId)).ToList();
            else
            {
                invitations = _context.Invitations.Include(i => i.EmailInvitation).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();
                recordsCount = _context.Invitations.Count();
            }

            var page = new Pagination
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
                Method = i.Method,
                Address = i.EmailInvitation.Email,
                CreatedAt = i.CreatedAt
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
                throw new ArgumentNullException(CommonMessage.InvalidData);

            Invitations invitation = InsertInvitation(invitationDto);

            string url = GetInvitationUrl(invitationDto.ApplicationId, invitation.InvitationId);

            await _emailRepository.SendEmailAsync(invitationDto, url);

            return Task.CompletedTask;
        }

        private Invitations InsertInvitation(InvitationsDto invitationDto)
        {
            Invitations invitation = new Invitations()
            {
                RecipientName = invitationDto.RecipientName,
                ApplicationId = Obfuscation.Decode(invitationDto.ApplicationId),
                PrivilageId = Obfuscation.Decode(invitationDto.PrivilageId),
                OfficerId = Obfuscation.Decode(invitationDto.OfficerId),
                InstitutionId = Obfuscation.Decode(invitationDto.InstitutionId),
                Method = "email",
                CreatedAt = DateTime.Now,
                EmailInvitation = new EmailInvitations
                {
                    Email = invitationDto.Address
                }
            };
            _context.Invitations.Add(invitation);
            _context.SaveChanges();

            return invitation;
        }

        private string GetInvitationUrl(string applicationId, int invitationId)
        {
            RegistrationForms registrationForm = _context.RegistrationForms.Where(r => r.ApplicationId == Obfuscation.Decode(applicationId)).FirstOrDefault();
            if (registrationForm == null)
                throw new KeyNotFoundException(CommonMessage.RegistrationFormUrlNotFound);

            string token = JsonConvert.DeserializeObject<InvitationTokenResponse>(GetAPI(_dependencies.GenerateInvitationTokenUrl).Content).invitationToken.ToString();
            return registrationForm.Url + "?inv=" + Obfuscation.Encode(invitationId) + "&tk=" + token;
        }

        private dynamic GetAPI(string url, string query = "")
        {
            UriBuilder uriBuilder = new UriBuilder(_appSettings.Host + url);
            uriBuilder = AppendQueryToUrl(uriBuilder, query);
            var client = new RestClient(uriBuilder.Uri);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == 0)
                throw new HttpListenerException(400, CommonMessage.ConnectionFailure);

            if (!response.IsSuccessful)
                throw new HttpListenerException((int)response.StatusCode, response.Content);

            return response;
        }

        private UriBuilder AppendQueryToUrl(UriBuilder baseUri, string queryToAppend)
        {
            if (baseUri.Query != null && baseUri.Query.Length > 1)
                baseUri.Query = baseUri.Query.Substring(1) + "&" + queryToAppend;
            else
                baseUri.Query = queryToAppend;
            return baseUri;
        }
    }
}
