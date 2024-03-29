﻿using InvitationsService.Models;
using InvitationsService.Models.ResponseModel;
using System.Threading.Tasks;

namespace InvitationsService.Abstraction
{
    public interface IInvitationsRepository
    {
        Task<dynamic> PostInvitation(InvitationsDto invitationsDto);
        dynamic DeleteInvitation(string invitationId);
        dynamic GetInvitation(string invitationId, Pagination pageInfo);
    }
}
