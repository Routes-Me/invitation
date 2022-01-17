using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using InvitationsService.Abstraction;
using InvitationsService.Models;
using InvitationsService.Models.DBModels;
using InvitationsService.Models.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InvitationsService.Controllers
{
    [ApiController]
    [ApiVersion( "1.0" )]
    [Route("v{version:apiVersion}/")]
    public class InvitationsController : ControllerBase
    {
        private readonly IInvitationsRepository _invitionRepository;
        private readonly InvitationsServiceContext _context;
        public InvitationsController(IInvitationsRepository InvitationRepository, InvitationsServiceContext context)
        {
            _invitionRepository = InvitationRepository;
            _context = context;
        }

        [HttpPost]
        [Route("invitations")]
        public async Task<IActionResult> Post(InvitationsDto invitationDto)
        {
            try
            {
                await _invitionRepository.PostInvitation(invitationDto);
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, new ErrorResponse{ error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ErrorResponse{ error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponse{ error = ex.Message });
            }
            return StatusCode(StatusCodes.Status202Accepted, CommonMessage.InvitationInserted);
        }

        [HttpDelete]
        [Route("invitations/{invitationId}")]
        public async Task<IActionResult> Delete(string invitationId)
        {
            try
            {
                Invitations invitation = _invitionRepository.DeleteInvitation(invitationId);
                _context.Invitations.Remove(invitation);
                await _context.SaveChangesAsync();
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponse{ error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ErrorResponse{ error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponse{ error = ex.Message });
            }
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpGet]
        [Route("invitations/{invitationId?}")]
        public IActionResult Get(string invitationId, [FromQuery] Pagination pageInfo)
        {
            GetResponse response = new GetResponse();
            try
            {
                response = _invitionRepository.GetInvitation(invitationId, pageInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponse{ error = ex.Message });
            }
            return StatusCode(StatusCodes.Status200OK, response);
        }
    }
}
