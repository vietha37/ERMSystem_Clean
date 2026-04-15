using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.DTOs.Common;
using ERMSystem.Application.Interfaces;
using ERMSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMSystem.API.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public AdminUsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] PaginationRequest request,
            [FromQuery] string? role,
            CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(role) &&
                !string.Equals(role, AppRole.Doctor, StringComparison.Ordinal) &&
                !string.Equals(role, AppRole.Receptionist, StringComparison.Ordinal))
            {
                return BadRequest("Role filter must be Doctor or Receptionist.");
            }

            var (items, totalCount) = await _userRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                role,
                request.TextSearch,
                ct);

            var mapped = items
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Role = u.Role
                });

            var result = new PaginatedResult<AdminUserDto>(
                mapped,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateAdminUserDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _userRepository.UsernameExistsAsync(dto.Username))
            {
                return Conflict($"Username '{dto.Username}' is already taken.");
            }

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                Username = dto.Username.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role
            };

            await _userRepository.AddAsync(user);

            var created = new AdminUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role
            };

            return CreatedAtAction(nameof(GetUsers), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateAdminUserDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            if (user.Role != AppRole.Doctor && user.Role != AppRole.Receptionist)
            {
                return BadRequest("Only Doctor and Receptionist accounts can be updated here.");
            }

            if (await _userRepository.UsernameExistsAsync(dto.Username, id, ct))
            {
                return Conflict($"Username '{dto.Username}' is already taken.");
            }

            user.Username = dto.Username.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                user.Role = dto.Role;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _userRepository.UpdateAsync(user, ct);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
        {
            var user = await _userRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            if (user.Role != AppRole.Doctor && user.Role != AppRole.Receptionist)
            {
                return BadRequest("Only Doctor and Receptionist accounts can be deleted here.");
            }

            await _userRepository.DeleteAsync(user, ct);
            return NoContent();
        }
    }
}
