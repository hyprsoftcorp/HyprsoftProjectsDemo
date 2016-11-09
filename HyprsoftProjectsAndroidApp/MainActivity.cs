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
using Android.Content.PM;

namespace HyprsoftProjectsAndroidApp
{
    [Activity(Label = "Hyprsoft Projects", MainLauncher = true, Icon = "@drawable/icon", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : Activity
    {
        #region Fields

        private bool _isLoading;
        private MobileServiceClient _mobileServiceClient = null;
        private ProjectsAdapter _projectsAdapter;
        private List<Project> _projectsDataSource;
        private IHubProxy _hubProxy;
        ProgressBar _progressBar;

        #endregion

        #region Methods

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            var toolbar = FindViewById<Toolbar>(Resource.Id.AppToolbar);
            toolbar.SetNavigationIcon(Resource.Drawable.icon);
            SetActionBar(toolbar);
            ActionBar.Title = GetString(Resource.String.ApplicationName);

            _projectsDataSource = new List<Project>();
            _projectsAdapter = new ProjectsAdapter(_projectsDataSource);
            _projectsAdapter.ItemClicked += OnProjectItemClicked;

            _progressBar = FindViewById<ProgressBar>(Resource.Id.Progressbar);
            var projectsRecycleView = FindViewById<RecyclerView>(Resource.Id.ProjectsRecyclerView);
            projectsRecycleView.SetLayoutManager(new LinearLayoutManager(this));
            projectsRecycleView.SetAdapter(_projectsAdapter);

            await LoadProjectsAsync();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.AppMenus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_refresh:
#pragma warning disable CS4014
                    LoadProjectsAsync();    // Fire and forget
#pragma warning restore CS4014
                    break;
                case Resource.Id.menu_about:
                    var alert = new AlertDialog.Builder(this);
                    var layout = LayoutInflater.Inflate(Resource.Layout.About, null);
                    alert.SetView(layout);
                    var version = "n/a";
                    try
                    {
                        version = PackageManager.GetPackageInfo(PackageName, 0).VersionName;
                    }
                    catch (PackageManager.NameNotFoundException)
                    {
                    }
                    ((TextView)layout.FindViewById(Resource.Id.AppVersion)).Text = $"{GetString(Resource.String.AppVersion)}: {version}";
                    alert.Create().Show();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private async Task LoadProjectsAsync()
        {
            if (_isLoading) return;

            try
            {
                _isLoading = true;
                _projectsDataSource.Clear();
                _projectsAdapter.NotifyDataSetChanged();

                _progressBar.Visibility = ViewStates.Visible;

                var authContext = new AuthenticationContext(Constants.HyprsoftAzureActiveDirectoryUrl);
                var clientCredentials = new ClientCredential(Constants.MobileServiceClientId, Constants.MobileServiceClientSecret);
                var authResult = await authContext.AcquireTokenAsync(Constants.MobileServiceUrl, clientCredentials);

                _mobileServiceClient?.Dispose();
                _mobileServiceClient = new MobileServiceClient(Constants.MobileServiceUrl);
                await _mobileServiceClient.LoginAsync(MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory, new JObject(new JProperty("access_token", authResult.AccessToken)));

                var table = _mobileServiceClient.GetTable<Project>();
                _projectsDataSource.AddRange(await table.OrderBy(p => p.CreatedAt).ToListAsync());

                var hubConnection = new HubConnection(Constants.MobileServiceUrl);
                hubConnection.Headers.Add("X-ZUMO-AUTH", _mobileServiceClient.CurrentUser.MobileServiceAuthenticationToken);
                _hubProxy = hubConnection.CreateHubProxy("ProjectsHub");
                _hubProxy.On<Project>("projectAdded", (project) => RunOnUiThread(() =>
                {
                    _projectsDataSource.Add(project);
                    _projectsAdapter.NotifyDataSetChanged();
                }));
                _hubProxy.On<string>("projectRemoved", (id) => RunOnUiThread(() =>
                {
                    var project = _projectsDataSource.SingleOrDefault(p => p.Id == id);
                    if (project != null)
                    {
                        _projectsDataSource.Remove(project);
                        _projectsAdapter.NotifyDataSetChanged();
                    }
                }));
                await hubConnection.Start();
                _projectsAdapter.NotifyDataSetChanged();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"An unexpected error occurred: {ex.Message}", ToastLength.Long).Show();
            }
            finally
            {
                _progressBar.Visibility = ViewStates.Gone;
                _isLoading = false;
            }
        }

        private void OnProjectItemClicked(object sender, int position)
        {
            try
            {
                var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(_projectsDataSource[position].WebsiteUri));
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Unable to launch the '{_projectsDataSource[position].WebsiteUri}' URI: {ex.Message}", ToastLength.Long).Show();
            }
        }

        #endregion
    }
}
