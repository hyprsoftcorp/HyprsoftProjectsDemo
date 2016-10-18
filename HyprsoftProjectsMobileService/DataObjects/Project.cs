using Microsoft.Azure.Mobile.Server;

namespace HyprsoftProjectsMobileService.DataObjects
{
    public class Project : EntityData
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string ImageUri { get; set; }

        public string LinkUri { get; set; }
    }
}