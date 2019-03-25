#! "netcoreapp2.0"

//uses: https://github.com/filipw/dotnet-script
//to install: dotnet tool install -g dotnet-script
//to use: dotnet script Services.csx
//to debug/develop in VS Code: 
//  see https://github.com/filipw/dotnet-script section 'Scaffolding' 
//  dotnet script init
//  ...that creates json files so that you can 'open folder' in VS Code

#r "nuget: Philadelphia.Common, 0.19.3.3"
#r "nuget: Philadelphia.CodeGen.ForClient, 0.19.3.3"
//for local template development instead those two lines above use: 
//#r "../../../Philadelphia.Common/bin/Debug/netcoreapp2.1/Philadelphia.Common.dll"
//#r "../../../Philadelphia.CodeGen.ForClient/bin/Debug/netcoreapp2.1/Philadelphia.CodeGen.ForClient.dll"

#r "../DependencyInjection.Domain/bin/Debug/netcoreapp2.1/DependencyInjection.Domain.dll"

Philadelphia.CodeGen.ForClient.ServiceInvokerGenerator.GenerateCode(
    "Services.cs",
    typeof(DependencyInjection.Domain.Dummy).Assembly,
    "DependencyInjection.Client");
Console.WriteLine("ok");
