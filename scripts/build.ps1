$TempFolder = [System.IO.Path]::GetTempPath()
$WorkingFolder = Join-Path $TempFolder $([string] [System.Guid]::NewGuid())
New-Item -ItemType Directory -Path $WorkingFolder

$PathToManufacturing = Join-Path -Resolve $PSScriptRoot ".." "Manufacturing"

$CSharp = Join-Path -Resolve $PathToManufacturing "src" "CSharp"

$Feed = Join-Path $WorkingFolder "feed"

dotnet build $(Join-Path $CSharp "Nuget" "Package")
dotnet pack -o $Feed $(Join-Path $CSharp "Nuget" "Package")

$nupkgName = Get-ChildItem -Path $Feed -Name -Filter "*.nupkg"

$TestProject = Join-Path $CSharp Nuget Tests DeviceAPITest DeviceAPITest DeviceAPITest.csproj
dotnet remove $TestProject package Microsoft.Azure.Sphere.DeviceAPI
dotnet add $TestProject package Microsoft.Azure.Sphere.DeviceAPI -s $Feed
dotnet build $TestProject

Remove-Item -Recurse $WorkingFolder