using HelpDeskHQ.Core.DTOs.Admin;
using HelpDeskHQ.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskHQ.API.Controllers
{
    [ApiController]
    [Route("api/admin/teams")]
    [Authorize(Roles = "Admin")]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamsController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var teams = await _teamService.GetAllAsync();
            return Ok(teams);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeamDto request)
        {
            var result = await _teamService.CreateAsync(request);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateTeamDto request)
        {
            var result = await _teamService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _teamService.DeleteAsync(id);
            return Ok(new { message = "Team deleted." });
        }
    }
}