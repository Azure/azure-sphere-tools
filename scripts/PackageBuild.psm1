<#
.SYNOPSIS
Invoke a dotnet command

.DESCRIPTION
Invoke the specified dotnet command, throwing an exception if the return code is non-zero
#>
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

<#
.SYNOPSIS
Build the device library

.DESCRIPTION
Build the device library found under the specified root with the specified version

.PARAMETER root
Path under which the library source can be found at root\Nuget\Package

.PARAMETER version
Version to apply to the built library
#>
function Build-Library
{
    param(
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $root,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $version
    )

    $package = Join-Path $root "Nuget" "Package"
    Write-Output "Building project at: ${package}"
    Invoke-Dotnet restore -v n -f $package 
    Invoke-Dotnet build -p:PackageVersion=$version --verbosity normal $package
}

<#
.SYNOPSIS
Build the device library package

.DESCRIPTION
Build a package from the previously build device library, and place the result in the given output folder

.PARAMETER outputFolder
Path to the location to save the finished package

.PARAMETER root
Path under which the library source can be found at root\Nuget\Package

.PARAMETER version
Version to apply to the package
#>
function Build-LibraryPackage
{
    param(
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $root,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $outputFolder,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $version
    )

    $package = Join-Path $root "Nuget" "Package"
    Write-Output "Packaging project at: ${package}"
    Invoke-Dotnet pack -o $outputFolder -p:PackageVersion=$version $package
}

<#
.SYNOPSIS
Publish the package to a local feed folder

.DESCRIPTION
Publish the specified package to a local package feed folder.

.PARAMETER outputFolder
Folder to which the build package has been written

.PARAMETER feed
Feed folder to publish the package

.PARAMETER root
Obsolete (do not use)

.PARAMETER version
Obsolete (do not use)
#>
function Publish-LocalPackage
{
    param(
        $root,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $outputFolder,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $feed,
        $version
    )

    $packages = Get-ChildItem -Filter "*.nupkg" $outputFolder
    if ($packages.Count -ne 1) {
        throw "Found more than one package, which was unexpected: ${packages}"
    }

    copy-item $packages.FullName -Destination $Feed
}

<#
.SYNOPSIS
Find nuget.config files

.DESCRIPTION
For a given project or project folder, find the Nuget.config file(s) that apply to it.
Searches up the directory hierachy from the specified project.

.PARAMETER project
Path to a project/project folder

.PARAMETER firstOnly
If set, return only the first found Nuget.config file
#>
function Find-NugetConfigs
{
    param(
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $project,
        [Parameter()] [switch] $firstOnly
    )

    $configs = @()
    $dir = Split-Path -Resolve $project
    while($dir) {
        $config = Join-Path $dir Nuget.config
        if (Test-Path $config) {
            if ($firstOnly) {
                return $config
            } else {
                $configs += $config
            }
        }
        $dir = Split-Path -Resolve $dir
    }

    $configs
}

<#
.SYNOPSIS
Build a project with package from the specified feed

.DESCRIPTION
For a project that uses the Microsoft.Azure.Sphere.DeviceAPI package, replace the package reference with one
to the specified version from the local feed, and then build it.

.PARAMETER project
Path to a project to build

.PARAMETER feed
Path to a local feed folder containing the package to use

.PARAMETER version
Package version to use

.PARAMETER publishLocation
Location to publish the build project (Optional)
#>
function Build-WithLocalPackage
{
    param(
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $project,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $feed,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $version,
        [Parameter()] [string] $publishLocation
    )

    $config = Find-NugetConfigs $project -firstOnly

    if (-not $config) {
        Write-Output "Cannot find nuget.config in $project directory hierarchy; falling back to default"
        Invoke-Dotnet nuget add source $feed --name "LocalFeed"
        Invoke-Dotnet nuget list source
    } else {
        Write-Output "Adding local feed ${config}:"
        Invoke-Dotnet nuget add source $feed --name "LocalFeed" --configfile $config
        Invoke-Dotnet nuget list source --configfile $config
    }

    Write-Output "Building project at ${project}"
    Write-Output "Replacing Microsoft.Azure.Sphere.DeviceAPI package with one at ${feed}"
    Invoke-Dotnet remove $project package Microsoft.Azure.Sphere.DeviceAPI
    Invoke-Dotnet add $project package Microsoft.Azure.Sphere.DeviceAPI --version $version --no-restore
    Invoke-Dotnet restore $project --verbosity normal 

    Write-Output "Using packages:"
    Invoke-Dotnet list $project package
    Write-Output "Building ${project}"
    Invoke-Dotnet build $project --verbosity normal

    Write-Output "Publishing to $publishLocation"
    if ($publishLocation) {
        Invoke-Dotnet publish $project --verbosity normal --output $publishLocation
    }

    Write-Output "Removing local feed"
    if (-not $config) {
        Invoke-Dotnet nuget remove source "LocalFeed"
    } else {
        Invoke-Dotnet nuget remove source "LocalFeed" --configfile $config
    }
}

function Build-Tests
{
    param(
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $root,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $feed,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $version,
        [Parameter()] [string] $publishLocation
    )

    $testProject = Join-Path $root Nuget Tests DeviceAPITest DeviceAPITest DeviceAPITest.csproj
    if ($publishLocation) {
        Build-WithLocalPackage $testProject $feed $version $(Join-Path $publishLocation tests)
    } else {
        Build-WithLocalPackage $testProject $feed $version
    }
}

function Build-Sample
{
    param(
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $root,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $feed,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $version,
        [Parameter()] [string] $publishLocation
    )

    $sample = Join-Path $root DeviceAPISample DeviceAPISample.csproj
    if ($publishLocation) {
        Build-WithLocalPackage $sample $feed $version $(Join-Path $publishLocation sample)
    } else {
        Build-WithLocalPackage $sample $feed $version
    }
}

function Get-PackageVersion {
    param(
        [Parameter()] [System.Version] $PackageVersion,
        [Parameter()] [string] [ValidateNotNullOrEmpty()] $PackageVersionSuffix
    )

    if (-not $PackageVersion) {
        $Version = "0.0.0.999-ci"
    } else {
        $Version = "${PackageVersion}"
        if ($PackageVersionSuffix) {
            $Version += "-${PackageVersionSuffix}"
        }
    }

    $Version
}


Export-ModuleMember `
    Build-Library, `
    Build-LibraryPackage, `
    Publish-LocalPackage, `
    Build-Tests, `
    Build-Sample, `
    Find-NugetConfigs, `
    Get-PackageVersion