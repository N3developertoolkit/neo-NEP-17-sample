$json = get-content .\.config\dotnet-tools.json | ConvertFrom-Json
$tools = $json.tools.psobject.Properties | %{$_.Name}
foreach ($tool in $tools) {
    dotnet tool update $tool
}
