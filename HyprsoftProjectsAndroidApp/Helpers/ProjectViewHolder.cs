using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

namespace HyprsoftProjectsAndroidApp.Helpers
{
    internal sealed class ProjectViewHolder : RecyclerView.ViewHolder
    {
        #region Constructors

        public ProjectViewHolder(View view, Action<int> itemClickCallback) : base(view)
        {
            Image = view.FindViewById<ImageView>(Resource.Id.ProjectImageImageView);
            Title = view.FindViewById<TextView>(Resource.Id.ProjectTitleTextView);
            Description = view.FindViewById<TextView>(Resource.Id.ProjectDescTextView);
            view.Click += (s, e) => itemClickCallback(AdapterPosition);
        }

        #endregion

        #region Properties

        public ImageView Image { get; private set; }

        public TextView Title { get; private set; }

        public TextView Description { get; private set; }

        #endregion
    }
}