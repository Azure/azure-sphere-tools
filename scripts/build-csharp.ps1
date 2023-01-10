<#
.SYNOPSIS
Build the NuGet device library, sample app and tests

.DESCRIPTION
Builds the NuGet device library with the given version, and publishes to a temporary local feed.
This feed is then used to build the sample app and tests.

.PARAMETER WorkingFolder
Optional working folder for the build

.PARAMETER PackageVersion
Optional package version to use (without suffix)

.PARAMETER PackageVersionSuffix
Optional package version suffix to use
#>

[CmdletBinding()]
param(
    [Parameter()] [string] [ValidateNotNullOrEmpty()] $WorkingFolder,
    [Parameter()] [System.Version] $PackageVersion,
    [Parameter()] [string] $PackageVersionSuffix
)

Set-StrictMode -Version Latest
Import-Module -Name $(Join-Path $PSScriptRoot PackageBuild.psm1 -Resolve)

Set-Location $(Join-Path $PSScriptRoot "..")

$Version = Get-PackageVersion -PackageVersion $PackageVersion -PackageVersionSuffix $PackageVersionSuffix

if (-not $WorkingFolder) {
    $TempFolder = [System.IO.Path]::GetTempPath()
    $WorkingFolder = Join-Path $TempFolder $([string] [System.Guid]::NewGuid())
    New-Item -ItemType Directory -Path $WorkingFolder | Out-Null
}

try {
    Write-Output "Using working folder ${WorkingFolder}"

    $Feed = Join-Path $WorkingFolder "feed"
    $Build = Join-Path $WorkingFolder "build"

    New-Item $Feed -ItemType Directory | Out-Null
    New-Item $Build -ItemType Directory | Out-Null

    Write-Output "Package feed at ${Feed}"
    Write-Output "Build folder at ${Build}"

    $PathToManufacturing = Join-Path -Resolve $PSScriptRoot ".." "Manufacturing"

    $CSharp = Join-Path -Resolve $PathToManufacturing "src" "CSharp"

    Build-Library $CSharp $Version
    Build-LibraryPackage $CSharp $Build $Version
    Publish-LocalPackage $CSharp $Build $Feed $Version

    Build-Tests $CSharp $Feed $Version
    Build-Sample $CSharp $Feed $Version
}
finally {
    if (-not $WorkingFolder) {
        Remove-Item -Recurse $WorkingFolder
    }
}