using Microsoft.AspNetCore.Mvc;

namespace DummyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MQTTController : Controller
    {
        // new mock DB reference
        MockDB mockDB = MockDB.GetInstance();
        // create logger
        private readonly ILogger<MQTTController> _logger;

        public MQTTController(ILogger<MQTTController> logger)
        {
            // register logger
            _logger = logger;
        }

        [HttpPost("subscribe/")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult Post([FromQuery] string sessionToken, [FromQuery] string topicName)
        {
            // prevent data race on DB
            lock (this.mockDB)
            {
                // TODO: implement format validation for the url variable topicName

                // try to find the session with provided sessionToken (tokenHash)
                var session_matched = mockDB.getSessions().FirstOrDefault(
                    s => s.TokenHash == sessionToken
                );
                // try to find topic with provided name
                var topic_matched = mockDB.getTopics().FirstOrDefault(
                    t => t.Name == topicName
                );

                // if there exists a session with sessionToken provided and a tpoic with Name = topicName
                if (session_matched != null && topic_matched != null)
                {
                    // get session associated user
                    var user_matched = mockDB.getUsers().FirstOrDefault(
                        u => u.Id == session_matched.UserId
                    );

                    // if user is in db
                    if (user_matched != null)
                    {
                        // check if the User is registered to the Topic
                        var user_to_topic_matched = mockDB.getUsersToTopics().FirstOrDefault(
                            u_to_t => u_to_t.UserId == user_matched.Id && u_to_t.TopicId == topic_matched.Id
                        );

                        if (user_to_topic_matched == null)
                        {
                            mockDB.getUsersToTopics().Add(
                                new UserToTopicMQTT
                                {
                                    Id = mockDB.getNewUserToTopicIdx(),
                                    UserId = user_matched.Id,
                                    TopicId = topic_matched.Id
                                }
                            );

                            // return status code: successfuly registered user to the topic
                            return Ok(); // TODO: return more verbose error code
                        }
                    }
                }

                return StatusCode(403); // TODO: return more verbose error code
            }
        }
    }
}
