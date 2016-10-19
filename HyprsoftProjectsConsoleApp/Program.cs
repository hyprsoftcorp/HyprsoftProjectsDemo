using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using ConsoleCommandProcessor.Library;
using System.Diagnostics;
using HyprsoftProjectsCommon;

namespace HyprsoftProjectsConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Commands
            const string AddProjectCommandName = "add";
            const string RemoveProjectCommandName = "rem";
            const string ListProjectsCommandName = "list";
            const string ProjectDetailsCommandName = "det";
            const string OpenLinkCommandName = "web";

            // Parameters
            const string IdParameterName = "id";
            const string TitleParameterName = "title";
            const string DescriptionParameterName = "description";
            const string ImageParameterName = "image";
            const string LinkParameterName = "link";

            var isAuthenticated = false;
            MobileServiceClient client = null;

            var manager = new CommandManager(async () =>
            {
                try
                {
                    Console.WriteLine("Connecting to the Hyprsoft Projects mobile app service...");
                    var authenticationContext = new AuthenticationContext(Constants.HyprsoftAzureActiveDirectoryUrl);
                    var authenticationResult = await authenticationContext.AcquireTokenAsync(Constants.MobileServiceUrl, new ClientCredential(Constants.MobileServiceClientId, Constants.MobileServiceClientSecret));

                    client = new MobileServiceClient(Constants.MobileServiceUrl);
                    var user = await client.LoginAsync(MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory, new JObject(new JProperty("access_token", authenticationResult.AccessToken)));
                    Console.WriteLine("Connected.");
                    isAuthenticated = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Mobile app service connection failed.  Reason: {ex.Message}");
                }
            }, async () =>
            {
                if (client == null) return;
                await client.LogoutAsync();
                client.Dispose();
            });

            // Add Project
            manager.AddCommand(AddProjectCommandName, new Command
            {
                Description = "Add a new user.",
                CanExecute = () => isAuthenticated,
                CantExecuteMessage = "Not connected.",
                Execute = async command =>
                {
                    try
                    {
                        var table = client.GetTable<Project>();
                        var project = new Project
                        {
                            Title = command.GetParameter(TitleParameterName).Value,
                            Description = command.GetParameter(DescriptionParameterName).Value,
                            ImageUri = command.GetParameter(ImageParameterName).Value,
                            LinkUri = command.GetParameter(LinkParameterName).Value
                        };
                        await table.InsertAsync(project);
                        Console.WriteLine($"Project '{command.GetParameter(TitleParameterName).Value}' with Id '{project.Id}' was successfully added.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unable to add the {command.GetParameter(TitleParameterName).Value} project.  Reason: {ex.Message}");
                    }
                }
            }).AddParameter(TitleParameterName, new Parameter
            {
                Prompt = "Title",
                Description = "Title of the new project.",
                CantValidateMessage = "Title cannot be null or whitespace.",
                Validate = value => Task.FromResult(!String.IsNullOrWhiteSpace(value))
            }).AddParameter(DescriptionParameterName, new Parameter
            {
                Prompt = "Description",
                Description = "Description of the new project.",
                CantValidateMessage = "Description cannot be null or whitespace.",
                Validate = value => Task.FromResult(!String.IsNullOrWhiteSpace(value))
            }).AddParameter(ImageParameterName, new Parameter
            {
                Prompt = "Image Uri",
                Description = "Image Uri of the new project.  ex. http://www.hyprsoft.com/images/image.jpg.",
                CantValidateMessage = "Image Uri is invalid.  The Uri must be absolute with a scheme of http or https.  *.jpg and *.png only.",
                Validate = value =>
                {
                    Uri uri;
                    if (Uri.TryCreate(value, UriKind.Absolute, out uri))
                        return Task.FromResult(uri.Scheme == "http" || uri.Scheme == "https" && (value.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || value.EndsWith(".png", StringComparison.OrdinalIgnoreCase)));
                    return Task.FromResult(false);
                }
            }).AddParameter(LinkParameterName, new Parameter
            {
                Prompt = "Link Uri",
                Description = "LinkUri of the new project.  ex. http://www.google.com/",
                CantValidateMessage = "Link Uri is invalid.    The Uri must be absolute with a scheme of http or https.",
                Validate = value =>
                {
                    Uri uri;
                    if (Uri.TryCreate(value, UriKind.Absolute, out uri))
                        return Task.FromResult(uri.Scheme == "http" || uri.Scheme == "https");
                    return Task.FromResult(false);
                }
            });

            var idParameter = new Parameter
            {
                Prompt = "Id",
                Description = "Id of a project.",
                CantValidateMessage = "Id cannot be null or whitespace.",
                Validate = value => Task.FromResult(!String.IsNullOrWhiteSpace(value))
            };

            // Remove Project
            manager.AddCommand(RemoveProjectCommandName, new Command
            {
                Description = "Remove an existing project.",
                CanExecute = () => isAuthenticated,
                CantExecuteMessage = "Not connected.",
                Execute = async command =>
                {
                    try
                    {
                        var table = client.GetTable<Project>();
                        var project = await table.LookupAsync(command.GetParameter(IdParameterName).Value);
                        await table.DeleteAsync(project);
                        Console.WriteLine($"Project '{project.Title}' was successfully removed.");
                    }
                    catch (MobileServiceInvalidOperationException)
                    {
                        Console.WriteLine($"Project with Id '{command.GetParameter(IdParameterName).Value}' does not exist.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }).AddParameter(IdParameterName, idParameter);

            // List Projects
            manager.AddCommand(ListProjectsCommandName, new Command
            {
                Description = "Displays available projects.",
                CanExecute = () => isAuthenticated,
                CantExecuteMessage = "Not connected.",
                Execute = async command =>
                {
                    Console.WriteLine("\nId | Title");
                    var table = client.GetTable<Project>();
                    foreach (var item in await table.OrderBy(p => p.CreatedAt).ToCollectionAsync())
                        Console.WriteLine($"{item.Id} | {item.Title}");
                }
            });

            // Project Details
            manager.AddCommand(ProjectDetailsCommandName, new Command
            {
                Description = "Display a project's details.",
                CanExecute = () => isAuthenticated,
                CantExecuteMessage = "Not connected.",
                Execute = async command =>
                {
                    try
                    {
                        var table = client.GetTable<Project>();
                        var project = await table.LookupAsync(command.GetParameter(IdParameterName).Value);
                        Console.WriteLine($"\nTitle: {project.Title}");
                        Console.WriteLine($"Description: {project.Description}");
                        Console.WriteLine($"Id: {project.Id}");
                        Console.WriteLine($"Updated: {project.UpdatedAt}");
                        Console.WriteLine($"Created: {project.CreatedAt}");
                        Console.WriteLine($"Image: {project.ImageUri}");
                        Console.WriteLine($"Link: {project.LinkUri}");
                    }
                    catch (MobileServiceInvalidOperationException)
                    {
                        Console.WriteLine($"Project with Id '{command.GetParameter(IdParameterName).Value}' does not exist.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }).AddParameter(IdParameterName, idParameter);

            // Open Project Link
            manager.AddCommand(OpenLinkCommandName, new Command
            {
                Description = "Opens a project's link.",
                CanExecute = () => isAuthenticated,
                CantExecuteMessage = "Not connected.",
                Execute = async command =>
                {
                    try
                    {
                        var table = client.GetTable<Project>();
                        var project = await table.LookupAsync(command.GetParameter(IdParameterName).Value);
                        Process.Start(project.LinkUri);
                    }
                    catch (MobileServiceInvalidOperationException)
                    {
                        Console.WriteLine($"Project with Id '{command.GetParameter(IdParameterName).Value}' does not exist.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }).AddParameter(IdParameterName, idParameter);

            Task.Run(async () => await manager.RunAsync()).Wait();
        }
    }
}
