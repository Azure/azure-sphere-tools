
function Build-Package($root, $outputFolder)
{
    Write-Host "Working in $root"
    $package = Join-Path $root "Nuget" "Package"
    Write-Host "Building $package"
    &dotnet build $package
    &dotnet pack -o $outputFolder -p:PackageVersion="9999.0.0.0" $package
}

function Build-Tests($root, $feed)
{
    # $nupkgName = Get-ChildItem -Path $feed -Name -Filter "*.nupkg"

    $TestProject = Join-Path $root Nuget Tests DeviceAPITest DeviceAPITest DeviceAPITest.csproj

    & dotnet nuget add source $feed

    & dotnet remove $TestProject package Microsoft.Azure.Sphere.DeviceAPI

    & dotnet add $TestProject package Microsoft.Azure.Sphere.DeviceAPI --version "9999.0.0.0"

    & dotnet restore $TestProject

    & dotnet build $TestProject
}

$TempFolder = [System.IO.Path]::GetTempPath()
$WorkingFolder = Join-Path $TempFolder $([string] [System.Guid]::NewGuid())
New-Item -ItemType Directory -Path $WorkingFolder
$Feed = Join-Path $WorkingFolder "feed"

$PathToManufacturing = Join-Path -Resolve $PSScriptRoot ".." "Manufacturing"

$CSharp = Join-Path -Resolve $PathToManufacturing "src" "CSharp"

Build-Package $CSharp $Feed
Build-Tests $CSharp $Feed

Remove-Item -Recurse $WorkingFolder