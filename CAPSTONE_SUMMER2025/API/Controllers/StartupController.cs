using System.Net;
using System.Text;
using API.DTO.AccountDTO;
using API.DTO.StartupDTO;
using API.Hubs;
using API.Service;
using API.Service.Interface;
using API.Utils.Constants;
using Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using UglyToad.PdfPig;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StartupController : ControllerBase
    {
        private readonly IStartupService _service;
        private readonly IAccountService _accountservice;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<StartupService> _logger;
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly CAPSTONE_SUMMER2025Context _context;

        private readonly IFileHandlerService _filehandler;



        public StartupController(IFileHandlerService fileHandler,IStartupService service, ILogger<StartupService> logger, IAccountService accountservice, IHubContext<MessageHub> hubContext, CAPSTONE_SUMMER2025Context context, IPermissionService permissionService)
        {
            _service = service;
            _logger = logger;
            _accountservice = accountservice;
            _hubContext = hubContext;
            _context = context;
            _filehandler = fileHandler;
            _permissionService = permissionService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateStartup([FromForm] CreateStartupRequest req)
        {
            var account = await _accountservice.GetAccountByAccountIDAsync(req.CreatorAccountId);
            if (!account.Status.Equals(AccountStatusConst.VERIFIED))
            {
                return BadRequest("Account chua verified!");
            }
            bool isMember = await _service.IsMemberOfAnyStartup(req.CreatorAccountId);
            if (isMember)
            {
                return BadRequest("Account da nam trong 1 startup!");
            }
            var startupId = await _service.CreateStartupAsync(req);
            return Ok(new { StartupId = startupId });
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllStartups(int pageNumber=1, int pageSize = 10, int? categoryId = null)
        {
            var result = await _service.GetAllStartupsAsync( pageNumber, pageSize,categoryId);
            return Ok(result);
        }

        //tạo chatroom
        [HttpPost("create-chatromm")]
        public async Task<IActionResult> Create([FromBody] CreateChatRoomDTO dto)
        {
            var hasPermission = await _permissionService.HasPermissionAsync(dto.CreatorAccountId, p => p.CanManageChatRoom);
            if (!hasPermission)
                throw new UnauthorizedAccessException("Bạn không có quyền quản lí chatroom");

            var room = await _service.CreateChatRoomAsync(dto);
            return Ok(new { message = "Created", roomId = room.ChatRoomId });
        }
        [HttpGet("check-membership")]
        public async Task<IActionResult> CheckMembership(int accountID)
        {
            if (accountID <= 0)
                return BadRequest(new { message = "accountID không hợp lệ!" });
            bool isMember = await _service.IsMemberOfAnyStartup(accountID);
            return Ok(new { isMember });
        }
        //thêm member
        [HttpPost("add-chatroom-members")]
        public async Task<IActionResult> AddMembers([FromBody] AddMembersDTO dto)
        {
            await _service.AddMembersToChatRoomAsync(dto);
            return Ok(new { message = "Đã thêm thành viên thành công." });
        }

        // lấy ra tất cả member thuộc startup
        [HttpGet("{startupId}/startup-members")]
        public async Task<IActionResult> GetStartupMembers(int startupId)
        {
            var members = await _service.GetMembersByStartupIdAsync(startupId);
            return Ok(members);
        }

        //lấy ra tất cả các members thuộc 1 chatroom
        [HttpGet("{chatRoomId}/chatroom-members")]
        public async Task<IActionResult> GetChatRoomMembers(int chatRoomId)
        {
            try
            {
                var result = await _service.GetMembersByChatRoomIdAsync(chatRoomId);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Handled exception");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                return StatusCode(500, new { message = "Lỗi hệ thống." });
            }
        }

        // lấy ra các chatroom mà account thuộc về
        [HttpGet("chatrooms/{accountId}")]
        public async Task<IActionResult> GetChatRoomsForAccount(
             int accountId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetChatRoomsByAccountIdAsync(accountId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Handled error");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error");
                return StatusCode(500, new { message = "Lỗi hệ thống." });
            }
        }


        // gửi message
         [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromForm] SendMessageRequest request)
        {
            try
            {
            // 1. Lưu message vào DB qua service
            var message = await _service.SendMessageAsync(request);

                // 2. Gửi realtime qua SignalR cho Receiver (và Sender nếu muốn đồng bộ nhiều tab)
                await _hubContext.Clients.Group(request.ChatRoomId.ToString())
         .SendAsync("ReceiveMessage", message);

            return Ok(message);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Gửi thất bại (xử lý được)");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
            _logger.LogError(ex, "Lỗi hệ thống");
            return StatusCode(500, new { message = "Lỗi hệ thống" });
            }
        }

        // lấy ra các message trong 1 chatroom
        [HttpGet("room-messages/{chatRoomId}")]
        public async Task<IActionResult> GetMessagesInRoom(
        int chatRoomId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetMessagesByRoomIdAsync(chatRoomId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Handled message error");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled server error");
                return StatusCode(500, new { message = "Lỗi hệ thống" });
            }
        }

        //tìm kiếm tin nhắn trong 1 chatroom
        [HttpGet("room-messages-search")]
        public async Task<IActionResult> SearchMessagesInRoom(
            int chatRoomId,
            string searchKey,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.SearchMessageAsync(chatRoomId, pageNumber, pageSize, searchKey);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Handled message error");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled server error");
                return StatusCode(500, new { message = "Lỗi hệ thống" });
            }
        }


        [HttpGet("stage")]
        public async Task<IActionResult> GetAllstage()
        {
            var stages = await _service.GetAllStagesAsync();
            return Ok(stages);
        }
        [HttpGet("search-account-by-email")]
        public async Task<IActionResult> SearchAccountByEmail([FromQuery] string keyword)
        {
            var accounts = await _service.SearchByEmailAsync(keyword);

            var result = accounts.Select(a => new {
                AccountId = a.AccountId,
                Email = a.Email,
                AvatarUrl = a.AccountProfile.AvatarUrl // Đảm bảo Include AccountProfile
            }).ToList();

            return Ok(result);
        }


        [HttpPut("update-member-title")]
        public async Task<IActionResult> UpdateMemberTitle([FromBody] UpdateMemberTitleRequest request)
        {
            var success = await _service.UpdateMemberTitleAsync(request);
            if (!success)
                return NotFound(new { message = "ChatRoomMember not found." });

            return Ok(new { message = "Member title updated successfully." });
        }
        [HttpPost("create-invite")]
        public async Task<IActionResult> CreateInvite([FromBody] CreateInviteDTO dto)
        {
            bool isMember = await _service.IsMemberOfAnyStartup(dto.Account_ID);
            if (isMember)
            {
                return BadRequest("Account da nam trong 1 startup!");
            }
            bool canInvite = await _service.CanInviteAgainAsync(dto.Account_ID, dto.Startup_ID);
            if (!canInvite)
                return BadRequest("Không thể mời lại: đã có invite chờ xử lý!");
            var invite = await _service.CreateInviteAsync(dto);
            return Ok(invite);
        }
        [HttpGet("startupid/{accountId}")]
        public async Task<IActionResult> GetStartupIdByAccountId(int accountId)
        {
            var startupId = await _service.GetStartupIdByAccountIdAsync(accountId);
            if (startupId == null)
                return NotFound("Account does not belong to any startup.");
            return Ok(startupId);
        }
        // GET: api/rolestartup/{roleId}
        [HttpGet("rolestartup/{roleId}")]
        public async Task<IActionResult> GetRole(int roleId)
        {
            var role = await _service.GetRoleAsync(roleId);
            if (role == null) return NotFound();
            return Ok(role);
        }
    
        [HttpGet("Getall-rolestartup/{startupId}")]
        public async Task<IActionResult> GetRolesByStartup(int startupId)
        {
            var roles = await _service.GetRolesByStartupAsync(startupId);
            return Ok(roles);
        }
    
        [HttpPost("Create-role")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            var role = await _service.CreateRoleAsync(dto);
            return Ok(role);
        }

        [HttpPut("Update-role")]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleDto dto)
        {
            var role = await _service.UpdateRoleAsync(dto);
            return Ok(role);
        }

        [HttpDelete("Delete-role/{roleId}")]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            var success = await _service.DeleteRoleAsync(roleId);
            if (!success) return NotFound();
            return Ok();
        }
        [HttpGet("search-members")]
        public async Task<IActionResult> SearchAndFilterMembers(
    [FromQuery] int startupId,
    [FromQuery] int? roleId,
    [FromQuery] string? search)
        {
            var members = await _service.SearchAndFilterMembersAsync(startupId, roleId, search);
            return Ok(members);
        }
        [HttpPost("kick-member")]
        public async Task<IActionResult> KickMember(int startupId, int accountId)
        {
            var success = await _service.KickMemberAsync(startupId, accountId);
            if (!success) return NotFound("Member not found or already removed.");
            return Ok("Kicked successfully.");
        }

        // POST: api/startupmember/out
        [HttpPost("out-startup")]
        public async Task<IActionResult> OutStartup(int accountId)
        {
            var success = await _service.OutStartupAsync(accountId);
            if (!success) return NotFound("Member not found or already removed.");
            return Ok("Left startup successfully.");
        }
        [HttpPut("update-member-role")]
        public async Task<IActionResult> UpdateMemberRole([FromBody] UpdateMemberRoleDto dto)
        {
            var result = await _service.UpdateMemberRoleAsync(dto.StartupId, dto.AccountId, dto.NewRoleId);
            if (!result) return NotFound("Member not found.");
            return Ok("Role updated successfully.");
        }

        //kick chatroom members
        [HttpDelete("kick-chatroom-members")]
        public async Task<IActionResult> KickMembers([FromBody] KickChatRoomMembersRequestDTO dto)
        {
            var success = await _service.KickMembersAsync(dto);
            if (!success)
                return Forbid("You don't have permission or members not found");

            return Ok("Members kicked successfully");
        }
        [HttpGet("startup/{startupId}/invites")]
        public async Task<IActionResult> GetInvitesByStartupPaged(int startupId, int pageNumber = 1, int pageSize = 10)
        {
            if (startupId <= 0)
                return BadRequest("StartupId is required and must be greater than 0.");
            var pagedResult = await _service.GetInvitesByStartupIdPagedAsync(startupId, pageNumber, pageSize);
            return Ok(pagedResult);
        }
        [HttpGet("invite/{inviteId}")]
        public async Task<IActionResult> GetInviteById(int inviteId)
        {
            if (inviteId <= 0)
                return BadRequest("InviteId must be greater than 0.");

            var invite = await _service.GetInviteByIdAsync(inviteId);
            if (invite == null)
                return NotFound("Invite not found.");

            return Ok(invite);
        }
        [HttpPost("invite/respond")]
        public async Task<IActionResult> RespondToInvite([FromBody] InviteRespondDto dto)
        {
            var success = await _service.UpdateInviteAsync(dto.InviteId, dto.Response);
            if (!success)
                return BadRequest("Invalid invite or invite not in pending status.");
            return Ok(new { message = $"Invite has been {dto.Response}ed." });
        }

        //lấy position requirment theo startupId
        [HttpGet("position-requirment")]
        public async Task<ActionResult<PagedResult<PositionRequirementResponseDto>>> GetPositionRequirementAll(
        [FromQuery] int startupId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetPositionRequirementPagedAsync(startupId, pageNumber, pageSize);
            return Ok(result);
        }

        //lấy ra position requirment theo id
        [HttpGet("position-requirment/{id}")]
        public async Task<ActionResult<PositionRequirementResponseDto>> GetPositionRequirementById(int id)
        {
            var dto = await _service.GetPositionRequirementByIdAsync(id);
            if (dto == null)
                return NotFound();

            return Ok(dto);
        }

        //tạo mới position requirment
        [HttpPost("position-requirment")]
        public async Task<ActionResult<PositionRequirementResponseDto>> CreatePositionRequirement([FromBody] PositionRequirementCreateDto model)
        {
            var createdDto = await _service.AddPositionRequirementAsync(model);
            return CreatedAtAction(nameof(GetPositionRequirementById), new { id = createdDto.PositionId }, createdDto);
        }

        //update position requirment theo id
        [HttpPut("position-requirment/{id}")]
        public async Task<IActionResult> UpdatePositionRequirement(int id, [FromBody] PositionRequirementUpdateDto model)
        {
            var success = await _service.UpdatePositionRequirementAsync(id, model);
            if (!success)
                return NotFound();

            return Ok("Update successfully");
        }

        //xóa position requirment
        [HttpDelete("position-requirment/{id}")]
        public async Task<IActionResult> DeletePositionRequirement(int id)
        {
            var success = await _service.DeletePositionRequirementAsync(id);
            if (!success)
                return NotFound();

            return Ok("delete successfully");
        }
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromQuery] int accountId, [FromQuery] int startupId)
        {
            var result = await _service.SubscribeAsync(accountId, startupId);
            if (result)
                return Ok(new { success = true, message = "Subscribe thành công!" });
            else
                return BadRequest(new { success = false, message = "Bạn đã subscribe startup này!" });
        }

        [HttpPost("unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromQuery] int accountId, [FromQuery] int startupId)
        {
            var result = await _service.UnsubscribeAsync(accountId, startupId);
            if (result)
                return Ok(new { success = true, message = "Unsubscribe thành công!" });
            else
                return NotFound(new { success = false, message = "Bạn chưa subscribe startup này!" });
        }

        //lấy ra chi tiết startup
        [HttpGet("{startupId}")]
        public async Task<IActionResult> GetStartupById(int startupId)
        {
            var result = await _service.GetStartupByIdAsync(startupId);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Startup not found"
                });
            }

            return Ok(new
            {
                message = "Get startup successfully",
                data = result
            });
        }

        //update startup
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStartup(int id, [FromForm] UpdateStartupDto dto)
        {
            var success = await _service.UpdateStartupAsync(id, dto);
            if (!success)
                return NotFound();

            return Ok("update thành công");
        }

        //tạo mới startup pitching
        [RequestSizeLimit(204857600)] // 200 MB
        [HttpPost("startup-pitching")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateStartupPitching([FromForm] StartupPitchingCreateDTO dto)
        {
            var created = await _service.AddStartupPitchingAsync(dto);
            return Ok(new
            {
                id = created.PitchingId,
                data = created
            });

        }

        //list ra các startup pistching theo startupid và type(nếu type bỏ trống thì chỉ theo startupId)
        [HttpGet("startup-pitching-by-startup/{startupId}")]
        public async Task<IActionResult> GetPitchingsByStartupAndType(int startupId, string? type)
        {
            var result = await _service.GetPitchingsByTypeAndStartupAsync(startupId, type);
            return Ok(result);
        }
        //xóa startup pitching
        [HttpDelete("startup-piching{id}")]
        public async Task<IActionResult> DeleteStartupPitching(int id)
        {
            var success = await _service.DeleteStartupPitchingAsync(id);
            if (!success)
                return NotFound(new { message = "Pitching not found." });

            return Ok("xóa thành công"); 
        }

        //update startup pitching
        [RequestSizeLimit(204857600)] // 200 MB
        [HttpPut("startup-pitching")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateStartupPitching([FromForm] StartupPitchingUpdateDTO dto)
        {
           
            var result = await _service.UpdateStartupPitchingAsync(dto.PitchingId, dto.File);
            if (!result) return NotFound();

            return Ok("cập nhật thành công");
        }

        //check xem account đã follow startup chưa nếu rồi trả về true
        [HttpGet("is-account-follow-startup")]
        public async Task<IActionResult> IsAccountSubcibeStartup(int accountId, int startupId)
        {
            var result = await _service.IsAccountSubcibeStartup(accountId, startupId);
            return Ok(result);
        }
    }
}

