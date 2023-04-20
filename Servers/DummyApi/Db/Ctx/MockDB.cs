using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;
using System.Timers;

namespace DummyApi.Controllers
{
    public sealed class MockDB
    {
        // set initial user index to 0
        private static int usersIdx = 0;
        // set initial user token index to 0
        private static int userTokensIdx = 0;
        // set initial device index to 0
        private static int devicesIdx = 0;
        // set initial MQTT topic index to 0
        private static int topicsIdx = 0;
        // set initial User to MQTT topic index to 0
        private static int userToTopicIdx = 0;
        // set initial Topic Data index to 0
        private static int topicDataIdx = 0;
        // set salt for password generation
        private static int passwordSalt = 12;
        // create mockup database table view for all users
        private static List<User> users = new List<User>();
        // create mockup database table view for existing sessions
        private static List<UserToken> sessions = new List<UserToken>();
        // create mockup database table view for registered devices
        private static List<RegisteredIoTDevice> devices = new List<RegisteredIoTDevice>();
        // create mockup database table view for existing topics
        private static List<TopicMQTT> topics = new List<TopicMQTT>();
        // create mockup database table view for topics to users relations
        private static List<UserToTopicMQTT> usersToTopics = new List<UserToTopicMQTT>();
        // create mockup database table view for topics to users relations
        private static List<TopicData> topicData = new List<TopicData>();

        // create timer for generating new topic data
        // generate new topic data on interval (in ms)
        private static System.Timers.Timer timerTopicData = new System.Timers.Timer(5000);

        // instance of the mock database object
        private static MockDB instance = null;


        // initialize the mock database
        private MockDB() {
            // create a test user for /login
            User loginUser = new User
            {
                Id = getNewUserIdx(),
                CreatedAt = DateTime.UtcNow,
                Email = "test.login@email.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password", passwordSalt)
            };
            users.Add(loginUser);
            devices.Add(new RegisteredIoTDevice
            {
                Id = getNewDeviceIdx(),
                UserId = loginUser.Id,
                RegistrationDate = DateTime.UtcNow,
                UniqueName = GenerateSSID("29:5f:d6:2f:0b:14"),
                Mac = "29:5f:d6:2f:0b:14",
                LEDState = false
            });
            devices.Add(new RegisteredIoTDevice
            {
                Id = getNewDeviceIdx(),
                UserId = loginUser.Id,
                RegistrationDate = DateTime.UtcNow,
                UniqueName = GenerateSSID("00:0a:93:bd:f0:1c"),
                Mac = "00:0a:93:bd:f0:1c",
                LEDState = false
            });


            // create a test user for /logout

            User logoutUser = new User
            {
                Id = this.getNewUserIdx(),
                CreatedAt = DateTime.UtcNow,
                Email = "test.logout@email.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password", passwordSalt),

            };
            users.Add(logoutUser);
            sessions.Add(
                new UserToken
                {
                    Id = getNewUserTokenIdx(),
                    UserId = logoutUser.Id,
                    CreationDate = DateTime.UtcNow,
                    TokenHash = "testLogoutSessionToken"
                }
            );

            // create a test topic for temperature readings
            topics.Add(new TopicMQTT
            {
                Id = getNewTopicIdx(),
                Name = "temperature"
            });
            TopicMQTT distanceTopic = new TopicMQTT
            {
                Id = getNewTopicIdx(),
                Name = "distance"
            };
            topics.Add(distanceTopic);


            // register timer callbacks
            timerTopicData.Elapsed += (Object source, ElapsedEventArgs e) => { generateRandomTopicData(distanceTopic.Name, 0, 1000); };
            timerTopicData.AutoReset = true;
            timerTopicData.Enabled = true;
            
        }


        // get instance of the database
        public static MockDB GetInstance ()
        { 
            if (instance == null)
                instance = new MockDB();
            return instance;
        }

        // Index counters
        public int getNewUserIdx()
        {
            return usersIdx++;
        }

        public int getNewUserTokenIdx()
        {
            return userTokensIdx++;
        }
        public int getNewDeviceIdx()
        {
            return devicesIdx++;
        }

        public int getNewTopicIdx()
        {
            return topicsIdx++;
        }

        public int getNewUserToTopicIdx()
        {
            return userToTopicIdx++;
        }

        public int getNewTopicDataIdx()
        {
            return topicDataIdx++;
        }

        // DB functions and macros mackups

        public String getNewUniqueSessionToken()
        {
            return Guid.NewGuid().ToString();
        }

        public int getPasswordSalt()
        {
            return passwordSalt;
        }

        // Database whole table select mockup methods

        public List<User> getUsers()
        {
            return users;
        }

        public List<UserToken> getSessions()
        {
            return sessions;
        }

        public List<RegisteredIoTDevice> getDevices()
        {
            return devices;
        }

        public List<TopicMQTT> getTopics()
        {
            return topics;
        }

        public List<UserToTopicMQTT> getUsersToTopics()
        {
            return usersToTopics;
        }

        public List<TopicData> GetTopicData()
        {
            return topicData;
        }

        // Helper method for populating mock DB with mock topic data

        private void generateRandomTopicData(string topicName, int minRange, int maxRange)
        {
            // get topic with name provided
            var topic = topics.FirstOrDefault(t => t.Name == topicName);

            // if topic exists and range limiters are correct
            if (topic != null && minRange <= maxRange)
            {   
                Random rand = new Random();

                // get current last generated index for topic data
                int beforeAddTopicDataMaxIdx = topicDataIdx; 
                
                // for each registered device
                devices.ForEach(d => {
                    // generate new random data
                    int newData = rand.Next(minRange, maxRange);
                    // add new topic data
                    topicData.Add(new TopicData
                    {
                        Id = getNewTopicDataIdx(),
                        TopicId = topic.Id,
                        DeviceId = d.Id,
                        CreatedAt = DateTime.UtcNow,
                        Data = newData
                    });
                });

                // get current last generated index for topic data
                int afterAddTopicDataMaxIdx = topicDataIdx;
                int newDataGeneratedCount = afterAddTopicDataMaxIdx - beforeAddTopicDataMaxIdx;
            }
        }

        // Unregistered IoTDevice internal methods mockup

        // Generate new Access Point SSID based on device MAC Address
        String GenerateSSID(String mac)
        {
            mac = mac.Replace(":", "");
            return "SmartDevice " + mac.Substring(4, 7-4) + "-" + mac.Substring(7, 12-7);
        }
    }
}
