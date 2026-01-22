using FoodOrderingCore.Request;
using FoodOrderingCore.Response;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingPRM392.Controllers
{
    [Route("api/tenants")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        public readonly ITenantRepository tenantRepository;
        public TenantController(ITenantRepository tenantRepository)
        {
            this.tenantRepository = tenantRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllTenantsAsync()
        {
            var tenants = await tenantRepository.GetAllTenantAsync();
            if (tenants == null) {
                return NotFound();
            }
            return Ok(new ParentResultResponse
            {
                Message = "Success",
                Result = tenants
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTenantByIdAsync(int id)
        {
            var tenant = await tenantRepository.GetTenantByIdAsync(id);
            if (tenant == null)
            {
                return NotFound();
            }
            return Ok(new ParentResultResponse
            {
                Message = "Success",
                Result = tenant
            });
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        public async Task<IActionResult> CreateTenantAsync([FromBody] TenantRequest tenantRequest)
        {
            var tenantId = await tenantRepository.CreateTenantAsync(tenantRequest);
            return Ok(new ParentResultResponse
            {
                Message = "Tenant created successfully",
                Result = tenantId
            });
        }
        [Authorize(Roles = "Admin, Manager")]
        [HttpPut]
        public async Task<IActionResult> UpdateTenantAsync([FromBody] TenantUpdateRequest tenantUpdateRequest)
        {
            var isUpdated = await tenantRepository.UpdateTenantAsync(tenantUpdateRequest);
            if (!isUpdated)
            {
                return BadRequest(new ParentResponse { Message = "Failed to update tenant" });
            }
            return Ok(new ParentResponse { Message = "Tenant updated successfully" });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTenantAsync(int id)
        {
            await tenantRepository.DeleteTenantAsync(id);
            return Ok(new ParentResponse { Message = "Tenant deleted successfully" });
        }
    }
}