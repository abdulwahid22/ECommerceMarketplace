using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.API.DTOs;
using ProductService.API.Helpers;

namespace ProductService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("public")]
        public ActionResult<ApiResponse<object>> Public()
        {
            return Ok(ApiResponse<object>.Ok(
                new
                {
                    service = "Product Service"
                },
                "Product Service public endpoint works."
            ));
        }

        [Authorize]
        [HttpGet("protected")]
        public ActionResult<ApiResponse<object>> Protected()
        {
            var userId = CurrentUserHelper.GetUserId(User);
            var roles = CurrentUserHelper.GetRoles(User);

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    userId,
                    roles
                },
                "Product Service protected endpoint works."
            ));
        }

        [Authorize(Roles = RoleNames.Seller)]
        [HttpGet("seller-only")]
        public ActionResult<ApiResponse<object>> SellerOnly()
        {
            return Ok(ApiResponse<object>.Ok(
                new
                {
                    message = "Only sellers can access this Product Service endpoint."
                },
                "Seller authorization works."
            ));
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpGet("admin-only")]
        public ActionResult<ApiResponse<object>> AdminOnly()
        {
            return Ok(ApiResponse<object>.Ok(
                new
                {
                    message = "Only admins can access this Product Service endpoint."
                },
                "Admin authorization works."
            ));
        }
    }
}