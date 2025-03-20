using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace InOut.Hubs
{
    public class RfidHub : Hub
    {
        public async Task SendTagUpdate(object data)
        {
            await Clients.All.SendAsync("ReceiveData", data);
        }
    }
}
