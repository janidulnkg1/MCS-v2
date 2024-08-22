using MCS_WEB_API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MCS_WEB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        readonly AppDbContext _context;
        readonly IConfiguration _config;


        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }



    }
}
