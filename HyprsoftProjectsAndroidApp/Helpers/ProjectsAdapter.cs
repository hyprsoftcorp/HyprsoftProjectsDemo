using System;
using System.Collections.Generic;
using System.Net.Http;

using Android.Views;
using Android.Graphics;
using Android.Support.V7.Widget;
using System.Threading.Tasks;
using HyprsoftProjectsCommon;
using Android.App;

namespace HyprsoftProjectsAndroidApp.Helpers
{
    internal class ProjectsAdapter : RecyclerView.Adapter
    {
        #region Fields

        private IReadOnlyList<Project> _items;
        private Dictionary<string, Bitmap> _bitmapCache = new Dictionary<string, Bitmap>();
        private Bitmap _placeholderBitmap;

        #endregion

        #region Constructors

        public ProjectsAdapter(IReadOnlyList<Project> items)
        {
            _items = items;
            _placeholderBitmap = BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.placeholder);
        }

        #endregion

        #region Events

        public event EventHandler<int> ItemClicked;

        #endregion

        #region Properties

        public override int ItemCount
        {
            get
            {
                return _items.Count;
            }
        }

        #endregion

        #region Methods

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            return new ProjectViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ProjectCardView, parent, false), OnItemClicked);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as ProjectViewHolder;

            holder.Title.Text = _items[position].Title;
            holder.Description.Text = _items[position].Description;
#pragma warning disable CS4014
            SetImageBitmapFromUrlAsync(holder, _items[position].ImageUri);  // Fire and forget
#pragma warning restore CS4014
        }

        protected virtual void OnItemClicked(int position)
        {
            ItemClicked?.Invoke(this, position);
        }

        private async Task SetImageBitmapFromUrlAsync(ProjectViewHolder holder, string url)
        {
            if (!_bitmapCache.ContainsKey(url))
            {
                _bitmapCache[url] = _placeholderBitmap;
                if (url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || url.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    using (var httpClient = new HttpClient())
                    {
                        try
                        {
                            var imageBytes = await httpClient.GetByteArrayAsync(url);
                            _bitmapCache[url] = await BitmapFactory.DecodeByteArrayAsync(imageBytes, 0, imageBytes.Length);
                        }
                        catch (Exception)
                        {
                            // Ignore any error as we will use our placeholder image.
                        }
                    }   // using http client
                }   // valid image Uri?
            }
            holder.Image.SetImageBitmap(_bitmapCache[url]);
        }

        #endregion
    }
}