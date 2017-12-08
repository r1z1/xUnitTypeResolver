
param(
  [string[]]$releases = 'Release',
  [string]$xunitPath = $null
)


$ErrorActionPreference = 'Stop'

$oldOFS = $OFS
$OFS = ', '
$releasesName = "$releases"
$OFS = $oldOFS

$projectRoot = Get-Item .
$projectName = $projectRoot.Name
Write-Host "/ Creating $projectName $releasesName"


# Check for external tools.
function Test-Tool {
  param(
    [string]$name,
    [string]$path
  )
  
  if( -not (Test-Path $path) ) { throw "Could not find $name at '$path'. Aborting." }
  
  $path
}

Write-Host "|  Loading external tools"

$rar = Test-Tool 'WinRar' "$env:tools\programs\WinRar\rar.exe"

$vs = Test-Tool 'VS2013' (Join-Path $env:VS120COMNTOOLS '..\IDE\devenv.exe')

if( -not $xunitPath ) { $xunitPath = './Imports/xunit/xunit.console.clr4.exe' }
$xunit = Test-Tool 'xUnit' $xunitPath


# Create each release.
foreach( $release in $releases ) {
  
  # Standardize release name.
  if( $release -eq 'Release' ) { $release = 'Release' }
  elseif( $release -eq 'Debug' ) { $release = 'Debug' }
  else { throw "Unrecognized release: $release" }
  
  Write-Host "| Starting $projectName $release"
  
  # Create exports directories.
  $exports = 'Exports'
  if( Test-Path $exports ) {
    $exportsDir = Get-Item $exports
  } else {
    Write-Host "|   Creating $exports directory"
    $exportsDir = New-Item $exports -type Directory
  }
  
  pushd $exports
  if( Test-Path $release ) {
    $releaseDir = Get-Item $release
    Remove-Item "$releaseDir/*" -include '*.log', '*.htm'
  } else {
    Write-Host "|   Creating $release directory"
    $releaseDir = New-Item $release -type Directory
  }
  popd
  
  
  # Build solution.
  $slnName = "$projectName.sln"
  $log = Join-Path $releaseDir "$projectName-build.log"
  $vsArgs = $slnName,'/rebuild',$release,'/out',$log
  
  Write-Host "|  Building '$slnName'"
  Write-Host "|   Waiting for '$vs' to complete..."
  
  # Start VS in a separate process so we can wait until it completes.
  $vsStartInfo = New-Object Diagnostics.ProcessStartInfo
  $vsStartInfo.FileName = $vs
  $vsStartInfo.UseShellExecute = $false
  $vsStartInfo.Arguments = "$vsArgs"
  
  $vsProcess = [Diagnostics.Process]::Start( $vsStartInfo )
  $vsProcess.WaitForExit( )
  $buildResult = $vsProcess.ExitCode
  $vsProcess.Close( )
  
  if( $buildResult -ne 0 ) { throw "$vs failed with exit code $buildResult `n  see '$log' for details" }
  
  
  # Get release version.
  $projectReleaseDir = Get-Item "./$projectName/bin/$release"
  $assemblyFile = Get-ChildItem $projectReleaseDir "$projectName.dll"
  $assemblyInfo = [Diagnostics.FileVersionInfo]::GetVersionInfo( $assemblyFile.FullName )
  [int[]]$assemblyVersion = $assemblyInfo.FileVersion.Split( '.' )
  $releaseVersionOffset = $(if( $assemblyVersion[2] ) { 2 } else { 1 })
  $releaseVersion = [string]::Join( '.', $assemblyVersion[0..$releaseVersionOffset] )
  
  
  # Test build.
  Write-Host "|  Testing release $releaseVersion"
  Write-Host "|   Waiting for '$xunit' to complete..."
  Write-Host
  
  $projectTestFile = Get-Item "./Test$projectName/bin/$release/Test$projectName.dll"
  $testLog = Join-Path $releaseDir "$projectName-test.htm"
  
  & $xunit $projectTestFile.FullName -html $testLog
  
  if( -not $? ) {
    Remove-Item "$releaseDir/*" -include '*.zip'
    $failureMessage = 'Cannot release version {0} - encountered {1} test failure{2}' -f $releaseVersion, $LastExitCode, $(if( $LastExitCode -ne 1 ) { 's' })
    throw $failureMessage
  }
  
  # Create archive files.
  Write-Host "|  Creating archives"
  
  function New-Archive {
    param(
      [string]$archive,
      [string]$rootDir,
      [string[]]$files
    )
    
    $archiveLog  = "$archive.log"
    $archiveName = "$archive.zip"
    $archivePath = (Join-Path $releaseDir "$archiveName")
    $archiveExportsPath = (Join-Path $exportsDir "$archiveName")
    
    Write-Host "|   Creating '$archiveName'"
    Write-Host "|   Waiting for '$rar' to complete..."
    if( Test-Path $archiveExportsPath ) { Remove-Item $archiveExportsPath }
    
    $prefix = (Join-Path $rootDir ' ' ).Trim( )
    
    $rargs = 'a','-as','-ed','-ierr'
    $rargs += "-ilog$archiveLog", "-ap$archiveName", $archivePath
    $rargs += ($files | % { $_.Replace( $prefix, '' ) })
    
    pushd $rootDir
    & $rar $rargs
    popd
    
    Copy-Item $archivePath $archiveExportsPath
    
    Write-Host 
  }
  
  
  # Create binary archive.
  $binArchiveName = "$projectName-v$releaseVersion-" + $release.ToLowerInvariant( )
  $binFiles = Get-ChildItem $projectReleaseDir -exclude 'xunit*'
  
  New-Archive $binArchiveName $projectReleaseDir $binFiles
  
  
  # Create source archive.
  if( $release -eq 'Release' ) {
    $ignoredFiles = '*.user','*.suo'
    if( Test-Path '.tfignore' ) {
      $ignoredFiles += @(Get-Content '.tfignore' | where { $_ -and $_[0] -ne '#' })
    }

    $sourceArchiveName = "$projectName-v$releaseVersion-source"
    $sourceFiles = Get-ChildItem $projectRoot -recurse | ? {
      -not $_.PSIsContainer
    } | ? {
      $fullname = $_.FullName
      -not ($exports,'bin','obj','packages','TestResults' | ? { $fullname -match ".*[\\/]$_.*" })
    } | ? {
      $name = $_.Name
      -not ($ignoredFiles | ? { $name -like $_ })
    } | % {
      $_.FullName
    }
    
    New-Archive $sourceArchiveName $projectRoot $sourceFiles
  }
  
}

Write-Host "\ Finished $projectName $releasesName"
Write-Host 
