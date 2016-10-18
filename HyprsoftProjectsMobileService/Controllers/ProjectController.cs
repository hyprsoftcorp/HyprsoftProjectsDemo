using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using HyprsoftProjectsMobileService.Models;
using HyprsoftProjectsMobileService.DataObjects;

namespace HyprsoftProjectsMobileService.Controllers
{
    [Authorize]
    public class ProjectController : TableController<Project>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<Project>(context, Request, true);
        }

        // GET tables/TodoItem
        public IQueryable<Project> GetAllProjects()
        {
            return Query();
        }

        // GET tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Project> GetProject(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Project> PatchProject(string id, Delta<Project> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/TodoItem
        public async Task<IHttpActionResult> PostProject(Project item)
        {
            Project current = await InsertAsync(item);
            Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<ProjectsHub>().Clients.All.projectAdded(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteProject(string id)
        {
            Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<ProjectsHub>().Clients.All.projectRemoved(id);
            return DeleteAsync(id);
        }
    }
}