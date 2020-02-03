[CmdletBinding()]
param(
    [string] $VcpkgDir
)

Set-StrictMode -Version 2
$ErrorActionPreference = 'Stop'

$packages = ('easyhook')
$triplets = ('x64-windows-vc142-mt')
$nugetPackageId = 'presentationtheme-aero-deps'
$nugetPackageVersion = '1.0.0'

$qualifiedPackages = $triplets | foreach { $t = $_; $packages | foreach { "${_}:${t}" } }

$rootDir = Split-Path -Parent $PSScriptRoot

if (! $VcpkgDir) {
    $VcpkgDir = Join-Path (Join-Path $rootDir 'build') 'deps'
    Write-Output "Using vcpkg in $VcpkgDir"
}

$vcpkg = Join-Path $VcpkgDir 'vcpkg.exe'

if (-not (Test-Path $vcpkg)) {
    if (-not (Test-Path (Join-Path $VcpkgDir '.git'))) {
        $git = Get-Command git

        Write-Output 'Downloading vcpkg'
        &$git clone -b master --single-branch --depth 1 https://github.com/microsoft/vcpkg.git $VcpkgDir
        if ($LASTEXITCODE -ne 0) { throw "Failed to git clone vcpkg repository" }
    }

    Write-Output 'Building vcpkg'
    & (Join-Path $VcpkgDir 'bootstrap-vcpkg.bat')
    if ($LASTEXITCODE -ne 0) { throw "vcpkg bootstrap failed" }
}

Copy-Item -Path (Join-Path $PSScriptRoot 'x64-windows-vc142-mt.cmake') `
          -Destination (Join-Path $VcpkgDir 'triplets') -Force

&$vcpkg install $qualifiedPackages
if ($LASTEXITCODE -ne 0) { throw "vcpkg install failed" }

&$vcpkg upgrade $qualifiedPackages --no-dry-run
if ($LASTEXITCODE -ne 0) { throw "vcpkg upgrade failed" }

&$vcpkg export $qualifiedPackages --nuget --nuget-version=$nugetPackageVersion --nuget-id=$nugetPackageId
if ($LASTEXITCODE -ne 0) { throw "vcpkg export failed" }

# Install the package to the solution packages directory
$nuget = Get-Command nuget -ErrorAction SilentlyContinue
if (! $nuget) {
    Write-Output 'Downloading nuget commandline client'
    $nuget = Join-Path (Join-Path $rootDir 'build') 'nuget.exe'
    Invoke-WebRequest -Uri https://dist.nuget.org/win-x86-commandline/v5.4.0/nuget.exe -OutFile $nuget
}

$packageDir = Join-Path $rootDir 'Source\packages'
&$nuget install $nugetPackageId -Version $nugetPackageVersion -Source $VcpkgDir -OutputDirectory $packageDir -NonInteractive -Verbosity quiet
if ($LASTEXITCODE -ne 0) { throw "nuget install failed" }
