using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudyCenter.System;
using StudyCenter.Models;

namespace StudyCenter.Controllers
{
    [ApiController]
    public class VerifyTokenController : ControllerBase
    {
        private readonly Authentication _authentication;

        public VerifyTokenController(Authentication authentication)
        {
            _authentication = authentication;
        }

        [HttpPost("api/auth/verifytoken")]
        public async Task<IActionResult> VerifyToken([FromBody] VerifyTokenRequest request)
        {
            UsersData result = await _authentication.VerifyToken(request.Token);

            if(result == null)
            {
                return Unauthorized(new {error = 8});
            }
            return Ok(new {status = 1});
        }
    }
}