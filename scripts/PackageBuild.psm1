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

function Build-Library
{
    param(
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $root,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $outputFolder,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $version
    )

    $package = Join-Path $root "Nuget" "Package"
    Write-Output "Building project at: ${package}"
    Invoke-Dotnet restore -v n -f $package 
    Invoke-Dotnet build -p:PackageVersion=$version --verbosity normal $package
}

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

function Publish-LocalPackage
{
    param(
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $root,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $outputFolder,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $feed,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $version
    )

    $packages = Get-ChildItem -Filter "*.nupkg" $outputFolder
    if ($packages.Count -ne 1) {
        throw "Found more than one package, which was unexpected: ${packages}"
    }

    copy-item $packages.FullName -Destination $Feed
}

function Find-NugetConfigs
{
    param(
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $project
    )

    $configs = @()
    $dir = Split-Path -Resolve $project
    while($dir) {
        $config = Join-Path $dir Nuget.config
        if (Test-Path $config) {
            $configs += $config
        }
        $dir = Split-Path -Resolve $dir
    }

    $configs
}

function Build-WithLocalPackage
{
    param(
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $project,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $feed,
        [Parameter(Mandatory=$true)] [ValidateNotNullOrEmpty()] [string] $version
    )
    Write-Output "Adding local feed:"
    Invoke-Dotnet nuget add source $feed --name "LocalFeed"

    $localConfigs = Find-NugetConfigs $(Get-Location)
    foreach($config in $localConfigs) {
        Write-Output "Config: $config"
        cat $config
        Write-Output "--------------------------"
    }

    Invoke-Dotnet nuget list source
    foreach($config in $(Find-NugetConfigs $project)) {
        Write-Output "Config: $config"
        cat $config
        Write-Output "--------------------------"
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
    Invoke-Dotnet nuget remove source LocalFeed
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

Export-ModuleMember `
    Build-Library, `
    Build-LibraryPackage, `
    Publish-LocalPackage, `
    Build-Tests, `
    Build-Sample,
    Find-NugetConfigs