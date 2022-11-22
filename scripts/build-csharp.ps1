
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

function Build-Package
{
    param(
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $root,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $outputFolder,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $version
    )

    Write-Output "Working in: ${root}"
    $package = Join-Path $root "Nuget" "Package"
    Write-Output "Building project at: ${package}"
    Invoke-Dotnet build -p:PackageVersion=$version $package
    Invoke-Dotnet pack -o $outputFolder -p:PackageVersion=$version $package
}

function Build-WithLocalPackage
{
    param(
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $project,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $feed,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $version
    )
    Write-Output "Adding local feed:"
    Invoke-Dotnet nuget add source $feed -n "LocalFeed"
    Invoke-Dotnet nuget list source

    Write-Output "Building project at ${project}"
    Write-Output "Replacing Microsoft.Azure.Sphere.DeviceAPI package with one at ${feed}"
    Invoke-Dotnet remove $project package Microsoft.Azure.Sphere.DeviceAPI
    Invoke-Dotnet add $project package Microsoft.Azure.Sphere.DeviceAPI --version $version
    Invoke-Dotnet restore $project
    Write-Output "Using packages:"
    Invoke-Dotnet list $project package
    Write-Output "Building ${project}"
    Invoke-Dotnet build $project
    Invoke-Dotnet nuget remove source "LocalFeed"
}

function Build-Tests
{
    param(
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $root,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $feed,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $version
    )

    $testProject = Join-Path $root Nuget Tests DeviceAPITest DeviceAPITest DeviceAPITest.csproj
    Build-WithLocalPackage $testProject $feed $version
}

function Build-Sample
{
    param(
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $root,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $feed,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $version
    )

    $sample = Join-Path $root DeviceAPISample DeviceAPISample.csproj
    Build-WithLocalPackage $sample $feed $version
}

if (-not $PackageVersion) {
    $Version = "999.0.0-ci"
} else {
    $Version = "${PackageVersion}"
    if ($PackageVersionSuffix) {
        $Version += "-${PackageVersionSuffix}"
    }
}

if (-not $WorkingFolder) {
    $TempFolder = [System.IO.Path]::GetTempPath()
    $WorkingFolder = Join-Path $TempFolder $([string] [System.Guid]::NewGuid())
    New-Item -ItemType Directory -Path $WorkingFolder | Out-Null
}

Write-Output "Using working folder ${WorkingFolder}"

$Feed = Join-Path $WorkingFolder "feed"

Write-Output "Package feed at ${Feed}"

$PathToManufacturing = Join-Path -Resolve $PSScriptRoot ".." "Manufacturing"

$CSharp = Join-Path -Resolve $PathToManufacturing "src" "CSharp"

Build-Package $CSharp $Feed $Version

Build-Tests $CSharp $Feed $Version
Build-Sample $CSharp $Feed $Version

if (-not $WorkingFolder) {
    Remove-Item -Recurse $WorkingFolder
}