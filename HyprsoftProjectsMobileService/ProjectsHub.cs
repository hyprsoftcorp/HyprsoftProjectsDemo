using HyprsoftProjectsMobileService.DataObjects;
using Microsoft.AspNet.SignalR;

namespace HyprsoftProjectsMobileService
{
    [Authorize]
    public class ProjectsHub : Hub
    {
        public void ProjectAdded(Project item)
        {
            Clients.All.projectAdded(item);
        }

        public void ProjectRemoved(string id)
        {
            Clients.All.projectRemoved(id);
        }
    }
}