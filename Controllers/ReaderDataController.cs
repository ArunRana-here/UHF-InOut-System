/*using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using InOut.Models;
using InOut.Hubs;

namespace InOut.Controllers
{
    [Route("api/reader")]
    [ApiController]
    public class ReaderDataController : ControllerBase
    {
        private static Dictionary<string, RfidTagData> _rfidTags = new();
        private static List<RfidTagData> _history = new();
        private readonly IHubContext<RfidHub> _hubContext;
        private readonly HttpClient _httpClient;

        // Allowed tags set
        public static HashSet<string> AllowedTags = new() { "1010", "B102", "2491", "B001", "A019", "A004", "A032" };
        private static readonly object _lock = new();
        private static DateTime _lastUpdate = DateTime.MinValue;

        public ReaderDataController(IHubContext<RfidHub> hubContext, HttpClient httpClient)
        {
            _hubContext = hubContext;
            _httpClient = httpClient;
        }

        [HttpGet("fetch")]
        public async Task<IActionResult> FetchRfidData()
        {
            var response = await _httpClient.GetStringAsync("http://192.168.1.150:9098/");
            var tags = ParseTags(response);

            lock (_lock)
            {
                DateTime now = DateTime.Now;
                foreach (var tag in tags)
                {
                    if (!AllowedTags.Contains(tag.EPC)) continue;

                    if (_rfidTags.TryGetValue(tag.EPC, out var existingTag))
                    {
                        existingTag.ReadCount++;
                        existingTag.Timestamp = now;
                        existingTag.Status = "IN";
                    }
                    else
                    {
                        tag.Status = "IN";
                        tag.Timestamp = now;
                        _rfidTags[tag.EPC] = tag;
                    }

                    _history.Insert(0, new RfidTagData
                    {
                        EPC = tag.EPC,
                        ReadCount = tag.ReadCount,
                        Timestamp = tag.Timestamp,
                        Status = tag.Status
                    });
                }

                foreach (var key in _rfidTags.Keys.ToList())
                {
                    var timeElapsed = (now - _rfidTags[key].Timestamp).TotalSeconds;

                    if (timeElapsed > 5 && timeElapsed <= 8)
                    {
                        _rfidTags[key].Status = "Far";
                    }
                    else if (timeElapsed > 8)
                    {
                        _rfidTags[key].Status = "OUT";
                    }
                }

                if ((now - _lastUpdate).TotalSeconds >= 3)
                {
                    _lastUpdate = now;
                    _ = Task.Run(async () =>
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveData", _rfidTags.Values.OrderByDescending(x => x.Timestamp));
                    });
                }
            }

            return Ok(_rfidTags.Values);
        }

        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            return Ok(_history);
        }

        [HttpPost("clear")]
        public async Task<IActionResult> ClearHistory()
        {
            lock (_lock)
            {
                _history.Clear();
                _rfidTags.Clear();
            }
            await _hubContext.Clients.All.SendAsync("ReceiveData", _rfidTags.Values);
            return Ok("History Cleared");
        }

        private List<RfidTagData> ParseTags(string data)
        {
            var tags = new List<RfidTagData>();
            try
            {
                var json = JsonDocument.Parse(data);
                var tagArray = json.RootElement.GetProperty("tags").EnumerateArray();
                foreach (var tag in tagArray)
                {
                    var epc = tag.GetProperty("ep").GetString();
                    var rc = tag.GetProperty("rc").GetInt32();
                    tags.Add(new RfidTagData { EPC = epc, ReadCount = rc });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing JSON: " + ex.Message);
            }
            return tags;
        }

        [HttpGet("allowed-tags")]
        public IActionResult GetAllowedTags()
        {
            return Ok(AllowedTags.ToList());
        }

        [HttpPost("allowed-tags/add")]
        public IActionResult AddAllowedTag([FromBody] string epc)
        {
            lock (_lock)
            {
                if (!AllowedTags.Contains(epc) && !string.IsNullOrEmpty(epc))
                {
                    AllowedTags.Add(epc);
                }
            }

            return Ok(AllowedTags.ToList());
        }

        [HttpDelete("allowed-tags/remove")]
        public IActionResult RemoveAllowedTag([FromQuery] string epc)
        {
            lock (_lock)
            {
                if (AllowedTags.Contains(epc))
                {
                    AllowedTags.Remove(epc);
                }
            }

            return Ok(AllowedTags.ToList());
        }
    }
}*/

//new Logic here is that 
// 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using InOut.Models;
using InOut.Hubs;

namespace InOut.Controllers
{
    [Route("api/reader")]
    [ApiController]
    public class ReaderDataController : ControllerBase
    {
        private static Dictionary<string, RfidTagData> _rfidTags = new();
        private static List<RfidTagData> _history = new();
        private readonly IHubContext<RfidHub> _hubContext;
        private readonly HttpClient _httpClient;

        // Dictionary for mapping EPCs to custom names
        private static readonly Dictionary<string, string> TagNames = new()
        {
            { "A019", "Spanner No.012" },
            { "B102", "Scissor No.005" },
            { "A032", "Screwdriver No.008" },
            { "1010","Pliers No.001"},
            { "B001","Wrench No.009"},
            { "A004","Chisel No.023"},
            { "2491","Drill No.001"}
        };

        public static HashSet<string> AllowedTags = new() { "1010", "B102", "2491", "B001", "A019", "A004", "A032" };
        private static readonly object _lock = new();
        private static DateTime _lastUpdate = DateTime.MinValue;

        public ReaderDataController(IHubContext<RfidHub> hubContext, HttpClient httpClient)
        {
            _hubContext = hubContext;
            _httpClient = httpClient;
        }

        [HttpGet("fetch")]
        public async Task<IActionResult> FetchRfidData()
        {
            var response = await _httpClient.GetStringAsync("http://192.168.1.150:9098/");
            var tags = ParseTags(response);

            lock (_lock)
            {
                DateTime now = DateTime.Now;
                foreach (var tag in tags)
                {
                    if (!AllowedTags.Contains(tag.EPC)) continue;

                    // Check if EPC has a custom name
                    if (TagNames.ContainsKey(tag.EPC))
                    {
                        tag.EPC = TagNames[tag.EPC];
                    }

                    if (_rfidTags.TryGetValue(tag.EPC, out var existingTag))
                    {
                        existingTag.ReadCount++;
                        existingTag.Timestamp = now;

                        // If previously marked as OUT, re-evaluate
                        if (existingTag.Status == "OUT" && (now - existingTag.Timestamp).TotalSeconds <= 2)
                        {
                            existingTag.Status = "Far";
                            existingTag.Timestamp = now.AddSeconds(3); // Grace period of 3 seconds
                        }
                        else
                        {
                            existingTag.Status = "IN";
                        }
                    }
                    else
                    {
                        tag.Status = "IN";
                        tag.Timestamp = now;
                        _rfidTags[tag.EPC] = tag;
                    }

                    _history.Insert(0, new RfidTagData
                    {
                        EPC = tag.EPC,
                        ReadCount = tag.ReadCount,
                        Timestamp = tag.Timestamp,
                        Status = tag.Status
                    });
                }

                foreach (var key in _rfidTags.Keys.ToList())
                {
                    var timeElapsed = (now - _rfidTags[key].Timestamp).TotalSeconds;

                    if (timeElapsed > 4 && timeElapsed <= 8)
                    {
                        _rfidTags[key].Status = "Far";
                    }
                    else if (timeElapsed > 8)
                    {
                        _rfidTags[key].Status = "OUT";
                    }
                }

                if ((now - _lastUpdate).TotalSeconds >= 3)
                {
                    _lastUpdate = now;
                    _ = Task.Run(async () =>
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveData", _rfidTags.Values.OrderByDescending(x => x.Timestamp));
                    });
                }
            }

            return Ok(_rfidTags.Values);
        }

        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            return Ok(_history);
        }

        [HttpPost("clear")]
        public async Task<IActionResult> ClearHistory()
        {
            lock (_lock)
            {
                _history.Clear();
                _rfidTags.Clear();
            }
            await _hubContext.Clients.All.SendAsync("ReceiveData", _rfidTags.Values);
            return Ok("History Cleared");
        }

        private List<RfidTagData> ParseTags(string data)
        {
            var tags = new List<RfidTagData>();
            try
            {
                var json = JsonDocument.Parse(data);
                var tagArray = json.RootElement.GetProperty("tags").EnumerateArray();
                foreach (var tag in tagArray)
                {
                    var epc = tag.GetProperty("ep").GetString();
                    var rc = tag.GetProperty("rc").GetInt32();
                    tags.Add(new RfidTagData { EPC = epc, ReadCount = rc });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing JSON: " + ex.Message);
            }
            return tags;
        }

        [HttpGet("allowed-tags")]
        public IActionResult GetAllowedTags()
        {
            return Ok(AllowedTags.ToList());
        }

        [HttpPost("allowed-tags/add")]
        public IActionResult AddAllowedTag([FromBody] string epc)
        {
            lock (_lock)
            {
                if (!AllowedTags.Contains(epc) && !string.IsNullOrEmpty(epc))
                {
                    AllowedTags.Add(epc);
                }
            }
            return Ok(AllowedTags.ToList());
        }

        [HttpDelete("allowed-tags/remove")]
        public IActionResult RemoveAllowedTag([FromQuery] string epc)
        {
            lock (_lock)
            {
                if (AllowedTags.Contains(epc))
                {
                    AllowedTags.Remove(epc);
                }
            }
            return Ok(AllowedTags.ToList());
        }
    }
}
