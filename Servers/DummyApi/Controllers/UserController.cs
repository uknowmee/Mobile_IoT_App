using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using BCrypt.Net;
using System.Net;

namespace DummyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        MockDB mockDB = MockDB.GetInstance();

        // create logger
        private readonly ILogger<UserController> _logger;

        
        public UserController(ILogger<UserController> logger) {
            // register logger
            _logger = logger;
        }


        [HttpGet("login/")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]    
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult Get([FromQuery] string email, [FromQuery] string password)
        {
            // prevent data race on DB
            lock (this.mockDB)
            {
                // try to find the user of email provided in url query
                var user_matched = mockDB.getUsers().FirstOrDefault(u => u.Email == email);
                // if user is in db and passwords match
                if (user_matched != null && BCrypt.Net.BCrypt.Verify(password, user_matched.PasswordHash))
                {
                    // create a new valid user token (essentaily a session identifier)
                    UserToken newSessionToken = new UserToken
                    {
                        // set new session id
                        Id = mockDB.getNewUserTokenIdx(),
                        // set foreign key to the matched user id
                        UserId = user_matched.Id,
                        // set new user token creation date to now
                        CreationDate = DateTime.UtcNow,
                        // generate new user token hash
                        TokenHash = mockDB.getNewUniqueSessionToken()
                    };
                    // add new session to the user sessions list
                    mockDB.getSessions().Add(newSessionToken);

                    // return only the tokenHash
                    return Ok(newSessionToken); // TODO: return more verbose error code
                }
                // return error code: wrong username or password
                return StatusCode(403); // TODO: return more verbose error code
            }
        }

        [HttpPut("logout/")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Put([FromQuery] string sessionToken)
        {
            // prevent data race on DB
            lock (this.mockDB)
            {
                // set user and sesssion as unmatched
                var session_matched = mockDB.getSessions().FirstOrDefault(
                    s => s.TokenHash == sessionToken
                );

                // if there exists a session with sessionToken provided
                if (session_matched != null)
                {
                    // get session associated user
                    var user_matched = mockDB.getUsers().FirstOrDefault(
                        u => u.Id == session_matched.UserId
                    );

                    // if user is in db
                    if (user_matched != null)
                    {
                        // remove the found session from the uers's sessions list
                        mockDB.getSessions().Remove(session_matched);
                        // return status code: successfuly closed user session
                        return Ok(); // TODO: return more verbose error code
                    }
                }
                // return error code: session does not exist
                return NotFound(); // TODO: return more verbose error code
            }
        }

        [HttpPost("register/")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult Post([FromQuery] string email, [FromQuery] string password)
        {
            // prevent data race on DB
            lock (this.mockDB)
            {
                // try to get a user from the mock db of email provided in the url query
                var user_matched = mockDB.getUsers().FirstOrDefault(u => u.Email == email);

                // if user email is not yet taken - no user with that email is in the db
                if (user_matched == null)
                {
                    // TODO: implement format validation for email and password

                    // create and save the user in the db
                    User newUser = new User
                    {
                        Id = mockDB.getNewUserIdx(),
                        CreatedAt = DateTime.UtcNow,
                        Email = email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, mockDB.getPasswordSalt()),
                    };

                    UserToken newSession = new UserToken
                    {
                        Id = mockDB.getNewUserTokenIdx(),
                        UserId = newUser.Id,
                        CreationDate = DateTime.UtcNow,
                        TokenHash = mockDB.getNewUniqueSessionToken()
                    };


                    mockDB.getUsers().Add(newUser);
                    mockDB.getSessions().Add(newSession);

                    // return a new session for the newly created user
                    return Ok(newSession); // TODO: return more verbose error code
                }

                // Can't create user with that email, it's already taken
                return StatusCode(403); // TODO: return more verbose error code
            }
        }
    }
}
