using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using HyprsoftProjectsAndroidApp.Helpers;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.SignalR.Client;
using HyprsoftProjectsCommon;

using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Widget;
using Android.Views;
using Android.Content;

namespace HyprsoftProjectsAndroidApp
{
    [Activity(Label = "Hyprsoft Projects", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        #region Fields

        private MobileServiceClient _client = null;
        private ProjectsAdapter _adapter;
        private List<Project> _projects;
        private IHubProxy _hubProxy;

        #endregion

        #region Methods

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            var toolbar = FindViewById<Toolbar>(Resource.Id.AppToolbar);
            SetActionBar(toolbar);
            ActionBar.Title = GetString(Resource.String.ApplicationName);

            _projects = new List<Project>();
            _adapter = new ProjectsAdapter(_projects);
            _adapter.ItemClicked += OnProjectItemClicked;

            var projectsRecycleView = FindViewById<RecyclerView>(Resource.Id.ProjectsRecyclerView);
            projectsRecycleView.SetLayoutManager(new LinearLayoutManager(this));
            projectsRecycleView.SetAdapter(_adapter);

            await LoadProjectsAsync();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.AppMenus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
#pragma warning disable CS4014
            LoadProjectsAsync();    // Fire and forget
#pragma warning restore CS4014
            return base.OnOptionsItemSelected(item);
        }

        private async Task LoadProjectsAsync()
        {
            var progress = FindViewById<ProgressBar>(Resource.Id.Progressbar);
            try
            {
                _projects.Clear();
                _adapter.NotifyDataSetChanged();

                progress.Visibility = ViewStates.Visible;

                var authContext = new AuthenticationContext(Constants.HyprsoftAzureActiveDirectoryUrl);
                var clientCredentials = new ClientCredential(Constants.MobileServiceClientId, Constants.MobileServiceClientSecret);
                var authResult = await authContext.AcquireTokenAsync(Constants.MobileServiceUrl, clientCredentials);

                _client?.Dispose();
                _client = new MobileServiceClient(Constants.MobileServiceUrl);
                await _client.LoginAsync(MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory, new JObject(new JProperty("access_token", authResult.AccessToken)));

                var table = _client.GetTable<Project>();
                _projects.AddRange(await table.OrderBy(p => p.CreatedAt).ToListAsync());

                var hubConnection = new HubConnection(Constants.MobileServiceUrl);
                hubConnection.Headers.Add("X-ZUMO-AUTH", _client.CurrentUser.MobileServiceAuthenticationToken);
                _hubProxy = hubConnection.CreateHubProxy("ProjectsHub");
                _hubProxy.On<Project>("projectAdded", (project) => RunOnUiThread(() =>
                {
                    _projects.Add(project);
                    _adapter.NotifyDataSetChanged();
                }));
                _hubProxy.On<string>("projectRemoved", (id) => RunOnUiThread(() =>
                {
                    var project = _projects.SingleOrDefault(p => p.Id == id);
                    if (project != null)
                    {
                        _projects.Remove(project);
                        _adapter.NotifyDataSetChanged();
                    }
                }));
                await hubConnection.Start();
                _adapter.NotifyDataSetChanged();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"An unexpected error occurred: {ex.Message}", ToastLength.Long).Show();
            }
            finally
            {
                progress.Visibility = ViewStates.Gone;
            }
        }

        private void OnProjectItemClicked(object sender, int position)
        {
            try
            {
                var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(_projects[position].LinkUri));
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Unable to launch the '{_projects[position].LinkUri}' URI: {ex.Message}", ToastLength.Long).Show();
            }
        }

        #endregion
    }
}
