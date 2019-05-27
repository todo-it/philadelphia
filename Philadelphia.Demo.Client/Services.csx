#! "netcoreapp2.2"

//uses: https://github.com/filipw/dotnet-script
//to install: dotnet tool install -g dotnet-script
//to use: dotnet script Services.csx
//to debug/develop in VS Code: 
//  see https://github.com/filipw/dotnet-script section 'Scaffolding' 
//  dotnet script init
//  ...that creates json files so that you can 'open folder' in VS Code

#r "../Philadelphia.Common/bin/Debug/netcoreapp2.2/Philadelphia.Common.dll"
#r "../Philadelphia.Demo.SharedModel/bin/Debug/netcoreapp2.2/Philadelphia.Demo.SharedModel.dll"
#r "../Philadelphia.CodeGen.ForClient/bin/Debug/netcoreapp2.2/Philadelphia.CodeGen.ForClient.dll"

Philadelphia.CodeGen.ForClient.ServiceInvokerGenerator.GenerateCode(
    "Services.cs",
    typeof(Philadelphia.Demo.SharedModel.SomeDto).Assembly,
    "Philadelphia.Demo.Client");
Console.WriteLine("ok");
