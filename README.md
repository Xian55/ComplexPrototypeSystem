# ComplexPrototypeSystem

The project consist of 3 major components:

All three project written in C# .NET Core 3.1 Framework.

* **Service**: Designed to be a Windows Service application. The Sensor purpose is to obtain *average CPU Usage* and *temperature*.

* **Server**: Web API with ASP.NET Core, gives access to the *Sensor Settings* and *Sensor Reports* while authenticated. Data stored in *SQLExpress* via Entity Framework.

* **Client**: Blazor WebAssembly client.

The **Service** and **Server** communicates via TCP protocol, to exchange the Sensor Reports and updating Sensor Settings such as polling interval.

A registered user can change the polling interval settings from the **Client**.

# Usage

* Pull down the repo
* Be sure **dotnet-ef** is installed, [info](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
* Compile the solution
* Run `dotnet ef database update` to initialize the database
* Publish **Service** project and create a Windows Service, [info](https://learn.microsoft.com/en-us/dotnet/core/extensions/windows-service)
* If necessary it is possible to adjust *server:ip* in *appsettings.json*
* Also adjust the *ConnectionString* in *appsettings.json* if necessary