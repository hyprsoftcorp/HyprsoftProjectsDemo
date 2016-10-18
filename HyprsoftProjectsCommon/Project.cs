using System;

namespace HyprsoftProjectsCommon
{
    public partial class Project
    {
        public DateTimeOffset? CreatedAt { get; set; }

        public bool Deleted { get; set; }

        public string Id { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public byte[] Version { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string ImageUri { get; set; }

        public string LinkUri { get; set; }
    }
}
