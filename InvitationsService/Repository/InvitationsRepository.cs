using InvitationsService.Abstraction;
using InvitationsService.Models;
using InvitationsService.Models.DBModels;
using InvitationsService.Models.ResponseModel;
using InvitationsService.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
        private readonly InvitationEmailSettings _emailSettings;

        public InvitationsRepository(IOptions<AppSettings> appSettings, IOptions<Dependencies> dependencies, IOptions<InvitationEmailSettings> invitationEmailSettings, InvitationsServiceContext context)
        {
            _appSettings = appSettings.Value;
            _dependencies = dependencies.Value;
            _emailSettings = invitationEmailSettings.Value;
            _context = context;
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
                invitations = _context.Invitations.Where(i => i.InvitationId == Obfuscation.Decode(invitationId)).ToList();
            else
            {
                invitations = _context.Invitations.Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();
                recordsCount = _context.Invitations.Count();
            }

            var page = new Pagination
            {
                offset = pageInfo.offset,
                limit = pageInfo.limit,
                total = recordsCount
            };
   
            dynamic invitationData = invitations.Select(i => new InvitationsDto {
                    InvitationId = Obfuscation.Encode(i.InvitationId),
                    RecipientName = i.RecipientName,
                    ApplicationId = Obfuscation.Encode(i.ApplicationId),
                    Address = i.Address,
                    Data = i.Data,
                    OfficerId = Obfuscation.Encode(i.OfficerId),
                    InstitutionId = Obfuscation.Encode(i.InstitutionId)
                }).ToList();       

            return new GetResponse
            {
                data = invitationData,
                pagination = page,
            };
        }

        public async Task<dynamic> PostInvitation(PostInvitationsDto invitationDto)
        {
            if (invitationDto == null || string.IsNullOrEmpty(invitationDto.OfficerId))
                throw new ArgumentNullException(CommonMessage.InvalidData);

            Invitations invitation = new Invitations()
            {
                OfficerId = Obfuscation.Decode(invitationDto.OfficerId),
                Address = invitationDto.Email,
                RecipientName = invitationDto.Name,
                Data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(invitationDto))
            };
            _context.Invitations.Add(invitation);
            _context.SaveChanges();

            string token = JsonConvert.DeserializeObject<InvitationTokenResponse>(GetAPI(_dependencies.GenerateInvitationTokenUrl).Content).invitationToken.ToString();
            string link = _emailSettings.DashboardUrl + "?inv=" + Obfuscation.Encode(invitation.InvitationId) + "&tk=" + token;

            await SendEmail(invitationDto.Email, link);

            return CommonMessage.InvitationInserted;
        }

        private Task SendEmail(string emailReceiver, string link)
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_emailSettings.From, _emailSettings.PW),
                EnableSsl = true,
            };

            client.Send(_emailSettings.From, emailReceiver, _emailSettings.Subject, link);

            return Task.CompletedTask;
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
