
[CmdletBinding()]
param(
    [Parameter()] [string] [ValidateNotNullOrEmpty()] $WorkingFolder,
    [Parameter()] [System.Version] $PackageVersion,
    [Parameter()] [string] [ValidateNotNullOrEmpty()] $PackageVersionSuffix
)

function Invoke-Dotnet()
{
    if ($args.Count -eq 0) {
        throw "Must supply args to dotnet command"
    }

    & dotnet $args

    $result = $LASTEXITCODE

    if ($result -ne 0) {
        throw "dotnet ${args} exited with result code ${result}"
    }
}

function Build-Package($root, $outputFolder, $version)
{
    Write-Host "Working in: ${root}"
    $package = Join-Path $root "Nuget" "Package"
    Write-Host "Building project at: ${package}"
    Invoke-Dotnet build $package
    Invoke-Dotnet pack -o $outputFolder -p:PackageVersion=$version $package
}

function Build-Tests($root, $feed, $version)
{
    $testProject = Join-Path $root Nuget Tests DeviceAPITest DeviceAPITest DeviceAPITest.csproj
    Write-Host "Building test project at ${testProject}"
    Write-Host "Replacing Microsoft.Azure.Sphere.DeviceAPI package with one at ${feed}"
    Invoke-Dotnet nuget add source $feed
    Invoke-Dotnet remove $testProject package Microsoft.Azure.Sphere.DeviceAPI
    Invoke-Dotnet add $testProject package Microsoft.Azure.Sphere.DeviceAPI --version $version
    Invoke-Dotnet restore $testProject
    Write-Host "Building ${testProject}"
    Invoke-Dotnet build $testProject
}

if (-not $PackageVersion) {
    $version = "0.0.0.0-ci"
} else {
    $version = "${PackageVersion}"
    if ($PackageVersionSuffix) {
        $version += "-${PackageVersionSuffix}"
    }
}

if (-not $WorkingFolder) {
    $TempFolder = [System.IO.Path]::GetTempPath()
    $WorkingFolder = Join-Path $TempFolder $([string] [System.Guid]::NewGuid())
    New-Item -ItemType Directory -Path $WorkingFolder | Out-Null
}

Write-Host "Using working folder ${WorkingFolder}"

$Feed = Join-Path $WorkingFolder "feed"

Write-Host "Package feed at ${Feed}"

$PathToManufacturing = Join-Path -Resolve $PSScriptRoot ".." "Manufacturing"

$CSharp = Join-Path -Resolve $PathToManufacturing "src" "CSharp"

Build-Package $CSharp $Feed $version
Build-Tests $CSharp $Feed $version

if (-not $WorkingFolder) {
    Remove-Item -Recurse $WorkingFolder
}