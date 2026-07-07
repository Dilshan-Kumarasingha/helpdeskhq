using HelpDeskHQ.Core.DTOs.Admin;
using HelpDeskHQ.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskHQ.API.Controllers
{
    [ApiController]
    [Route("api/admin/sla-policies")]
    [Authorize(Roles = "Admin")]
    public class SlaPoliciesController : ControllerBase
    {
        private readonly ISlaPolicyService _slaPolicyService;

        public SlaPoliciesController(ISlaPolicyService slaPolicyService)
        {
            _slaPolicyService = slaPolicyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var policies = await _slaPolicyService.GetAllAsync();
            return Ok(policies);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSlaPolicyDto request)
        {
            var result = await _slaPolicyService.CreateAsync(request);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateSlaPolicyDto request)
        {
            var result = await _slaPolicyService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _slaPolicyService.DeleteAsync(id);
            return Ok(new { message = "SLA policy deleted." });
        }
    }
}