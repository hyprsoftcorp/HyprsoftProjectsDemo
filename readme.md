# Hyprsoft Projects Demo

This repository demonstrates managing and displaying recent web projects completed by [Hyprsoft Corporation](http://www.hyprsoft.com/). It uses the following technologies: [Azure Mobile Apps](https://azure.microsoft.com/en-us/services/app-service/mobile/), [Azure SQL Database](https://azure.microsoft.com/en-us/services/sql-database/), [Azure Active Directory](https://www.microsoft.com/en-us/cloud-platform/azure-active-directory) authentication (client credentials flow), [SignalR](http://signalr.net/), a Windows console app, and a native Android app built using [Xamarin](https://www.xamarin.com/).  The solution contains 4 projects.

1. **HyprsoftProjectsAndroidApp** - A native Android 6.0 app using AAD authentication to authenticate with the mobile service backend which populates an Android [RecyclerView](https://developer.android.com/reference/android/support/v7/widget/RecyclerView.html) with projects using the Azure Mobile App client.  Projects are kept in sync with the mobile service backend using SignalR.
2. **HyprsoftProjectsCommon** - A Portable Class Library (PCL) for shared code.
3. **HyprsoftProjectsConsoleApp** - .NET Framework 4.61 Windows console app to list, add, and remove projects via a command line interface. 
4. **HyprsoftProjectsMobileService** - Azure Mobile App with an ASP.NET backend.  This service uses SignalR to sync new and removed projects.

## Screenshots

### Native Android App
![Native Android App](https://cdn.rawgit.com/hyprsoftcorp/HyprsoftProjectsDemo/master/Images/android.jpg "Native Android App")
![Native Android App](https://cdn.rawgit.com/hyprsoftcorp/HyprsoftProjectsDemo/master/Images/android_about.jpg "Native Android App")

### Windows Console App
![Windows Console App](https://cdn.rawgit.com/hyprsoftcorp/HyprsoftProjectsDemo/master/Images/console.jpg "Windows Console App")

