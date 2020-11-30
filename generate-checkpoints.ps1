dotnet tool restore
dotnet build ./contract
dotnet nxp3 reset -f
dotnet nxp3 transfer neo 100000 genesis apoc-owner
dotnet nxp3 transfer gas 100000 genesis apoc-owner
dotnet nxp3 contract deploy ./contract/bin/Debug/netstandard2.1/Apoc.nef apoc-owner
dotnet nxp3 checkpoint create checkpoints/invoke-deploy -f
dotnet nxp3 contract invoke ./invoke-files/deploy.neo-invoke.json apoc-owner
dotnet nxp3 checkpoint create checkpoints/invoke-mint -f
dotnet nxp3 contract invoke ./invoke-files/mint.neo-invoke.json apoc-owner
