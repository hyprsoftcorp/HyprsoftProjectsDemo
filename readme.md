#Hyprsoft Projects Demo
A sample Visual Studio 2015 solution utilizing Azure Mobile Apps, AAD authentication, SignalR, a Windows console app, and a native Android app build using Xamarin.  The solution contains 4 projects.

1. **HyprsoftProjectsAndroidApp** - A native Android 6.0 app using Azure Active Directory Authentication to authenticate with the mobile service backend which populates a RecyclerView with projects using the Azure Mobile App Client.  Projects are kept in sync with the mobile service backend using SignalR.
2. **HyprsoftProjectsCommon** - A Portable Class Library (PCL) for shared code.
3. **HyprsoftProjectsConsoleApp** - .NET Framework 4.61 Windows console app to list, add, and remove projects via a command line interface. 
4. **HyprsoftProjectsMobileService** - Azure Mobile App with an ASP.NET backend.  This service uses SignalR to sync new and removed projects.

##Screenshots

###Native Android App
![Native Android App](https://cdn.rawgit.com/hyprsoftcorp/HyprsoftProjectsDemo/master/Images/android.jpg "Native Android App")

###Windows Console App
![Windows Console App](https://cdn.rawgit.com/hyprsoftcorp/HyprsoftProjectsDemo/master/Images/console.jpg "Windows Console App")
