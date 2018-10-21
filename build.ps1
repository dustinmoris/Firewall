# ----------------------------------------------
# Build script
# ----------------------------------------------

param
(
    [switch] $Release,
    [switch] $ExcludeTests,
    [switch] $Pack,
    [switch] $ClearOnly
)

# ----------------------------------------------
# Main
# ----------------------------------------------

$ErrorActionPreference = "Stop"

Import-module "$PSScriptRoot/.psscripts/build-functions.ps1" -Force

Write-BuildHeader "Starting Firewall build script"

if ($ClearOnly.IsPresent)
{
    Remove-OldBuildArtifacts
    return
}

$lib   = "./src/Firewall/Firewall.csproj"
$tests = "./tests/Firewall.Tests/Firewall.Tests.csproj"
$sample = "./samples/BasicApp/BasicApp.csproj"

$version = Get-ProjectVersion $lib
Update-AppVeyorBuildVersion $version

if (Test-IsAppVeyorBuildTriggeredByGitTag)
{
    $gitTag = Get-AppVeyorGitTag
    Test-CompareVersions $version $gitTag
}

Write-DotnetCoreVersions
Remove-OldBuildArtifacts

$configuration = if ($Release.IsPresent) { "Release" } else { "Debug" }

Write-Host "Building Firewall..." -ForegroundColor Magenta
dotnet-build $lib "-c $configuration"

if (!$ExcludeTests.IsPresent -and !$Run.IsPresent)
{
    Write-Host "Building and running tests..." -ForegroundColor Magenta

    dotnet-build $tests
    dotnet-test  $tests
}

Write-Host "Building samples..." -ForegroundColor Magenta
dotnet-build $sample "-c $configuration"

if ($Pack.IsPresent)
{
    Write-Host "Packaging Firewall NuGet package..." -ForegroundColor Magenta

    dotnet-pack $lib "-c $configuration"
}

Write-SuccessFooter "Firewall build completed successfully!"