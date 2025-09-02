using MI.CRM.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MI.CRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly MicrmContext _context;

        public TestController(MicrmContext context)
        {
            _context = context;
        }

        // 1. Simple ping endpoint
        [HttpGet("ping")]
        [AllowAnonymous]
        public IActionResult Ping()
        {
            return Ok(new { message = "API is running!" });
        }

        // 2. Test database connection
        [HttpGet("dbtest")]
        [AllowAnonymous]
        public IActionResult DbTest()
        {
            try
            {
                // Try to get count of users to test DB connectivity
                var userCount = _context.Users.Count();
                return Ok(new { message = "Database connected successfully", usersCount = userCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Database connection failed", error = ex.Message });
            }
        }

        // 3. Return the connection string (for testing only!)
        [HttpGet("connectionstring")]
        [AllowAnonymous]
        public IActionResult GetConnectionString()
        {
            try
            {
                var conn = _context.Database.GetConnectionString();
                return Ok(new { connectionString = conn });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get connection string", error = ex.Message });
            }
        }
    }
}
