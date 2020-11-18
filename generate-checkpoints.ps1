dotnet build ./contract
dotnet build C:\Users\harry\Source\neo\seattle\express\src\nxp3\nxp3.csproj
dotnet run -p C:\Users\harry\Source\neo\seattle\express\src\nxp3\nxp3.csproj --no-build -- reset -f
dotnet run -p C:\Users\harry\Source\neo\seattle\express\src\nxp3\nxp3.csproj --no-build -- transfer neo 100000 genesis apoc-owner
dotnet run -p C:\Users\harry\Source\neo\seattle\express\src\nxp3\nxp3.csproj --no-build -- transfer gas 100000 genesis apoc-owner
dotnet run -p C:\Users\harry\Source\neo\seattle\express\src\nxp3\nxp3.csproj --no-build -- contract deploy ./contract/bin/Debug/netstandard2.1/Apoc.nef apoc-owner
# dotnet run -p C:\Users\harry\Source\neo\seattle\express\src\nxp3\nxp3.csproj --no-build -- checkpoint create checkpoints/invoke-deploy -f
dotnet run -p C:\Users\harry\Source\neo\seattle\express\src\nxp3\nxp3.csproj --no-build -- contract invoke ./invoke-files/deploy.neo-invoke.json apoc-owner --trace
# dotnet run -p C:\Users\harry\Source\neo\seattle\express\src\nxp3\nxp3.csproj --no-build -- checkpoint create checkpoints/invoke-mint -f
# dotnet run -p C:\Users\harry\Source\neo\seattle\express\src\nxp3\nxp3.csproj --no-build -- contract invoke ./invoke-files/mint.neo-invoke.json apoc-owner
