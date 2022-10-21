$TempFolder = [System.IO.Path]::GetTempPath()
$WorkingFolder = Join-Path $TempFolder $([string] [System.Guid]::NewGuid())
New-Item -ItemType Directory -Path $WorkingFolder

$PathToManufacturing = Join-Path -Resolve $PSScriptRoot ".." "Manufacturing"

$CSharp = Join-Path -Resolve $PathToManufacturing "src" "CSharp"

$Feed = Join-Path $WorkingFolder "feed"

dotnet build $(Join-Path $CSharp "Nuget" "Package")
dotnet pack -o $Feed -p:PackageVersion="9999.0.0.0" $(Join-Path $CSharp "Nuget" "Package") 

$nupkgName = Get-ChildItem -Path $Feed -Name -Filter "*.nupkg"

$TestProject = Join-Path $CSharp Nuget Tests DeviceAPITest DeviceAPITest DeviceAPITest.csproj

dotnet nuget add source $Feed

Write-Host "::debug::Removing package from test project"

dotnet remove $TestProject package Microsoft.Azure.Sphere.DeviceAPI

Write-Host "::debug::Adding locally built package to test project"

dotnet add $TestProject package Microsoft.Azure.Sphere.DeviceAPI --version "9999.0.0.0"

Write-Host "::debug::Restoring packages for test project"

dotnet restore $TestProject

Write-Host "::debug::Building test project"

dotnet build $TestProject

Remove-Item -Recurse $WorkingFolder