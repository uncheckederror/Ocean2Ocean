# Ocean2Ocean
Enter your daily step count and track your team's progress as you walk together from Ocean to Ocean.

[Try it out here!](https://ocean2ocean20200530093132.azurewebsites.net/KCIT)

# How it's built
This product is composed of three projects:
* DataAccess is a shared library for data access
* Mvc is a server-side rendered front-end
* Tests is a testing project for the Mvc and DataAccess projects

The Data Access project is a dotnet standard 2.0 library that uses a micro-ORM called [Dapper](https://github.com/StackExchange/Dapper) to make queries against an Azure SQL instance.

The Mvc project uses the ASP.NET Core 3.1 framework to execute the business logic of this application and server-side rendering to handle the pages.

Do to how computationally expensive geospatial operations are, in this application we handle all of them in client-side Javascript using the [ArcGIS JS](https://developers.arcgis.com/javascript/) library to render the interactive map and the [Turf.js](https://turfjs.org/) library to manipulate the geometry of the route and calculate the distance covered.

This allows us to calculate the number of steps in the route and the two segments representing the distance we have traveled and the distance we have left to cover.

The production version of this application is hosted on an Azure App Service instance and supported by the smallest and cheapest Azure SQL instance. The tiles for the map use the OpenStreetMaps dataset and are provided by Mapbox using their free service tier. Bootstrap 4 is the CSS framework used to style the non-map HTML elements.

# Setup Instructions
* Clone this repositiory.
* Open the .sln Solution file in Visual Studio 2019.
* Add the required Azure SQL using [dotnet user-secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.1&tabs=windows). ( Contact the repo owner over encrypted messaging for the credentials or provide your own. )
* Build the application and run the tests to verify that everything works.
* Run the application in IIS Express to host it locally.

 # How to Contribute
 Open a new issue and then make a pull request assocated with that inital issue.
