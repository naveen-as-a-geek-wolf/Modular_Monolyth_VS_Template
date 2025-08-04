
<#
.Description
This script is used to migrate database to latest version on local dev env's
.PARAMETER action
Determines which action flyway needs to perform
.PARAMETER runSampleData
Indicate whether to execute sample data against the service specified
.PARAMETER dropDatabase
Indicate whether to drop and re-create database while executing migration
.PARAMETER setBaselineOnMigrate
Indicate flyway to create its baseline when executed against a new database

.EXAMPLE
PS> .\runDevFlyway.ps1 -action migrate -runSampleData $true -createDatabase $true  
.SYNOPSIS
Used to migrate database to latest version on local dev env's.
#>

param(
	[Parameter(Position = 0)]
	[ValidateSet("migrate", "info", "validate", "repair", "clean", "cleanAndMigrate")]
	[string]$action = "migrate",

	[Parameter(Mandatory = $true, Position = 1)]
	[string]$module,

	[Parameter(Position = 2)]
	[ValidateSet($true, $false)]
	[bool]$createDatabase = $false,

	[Parameter(Position = 3)]
	[ValidateSet($true, $false)]
	[bool]$runSampleData = $false,

	[Parameter(Position = 4)]
	[ValidateSet($true, $false)]
	[bool]$setBaselineOnMigrate = $false,
	
	[Parameter(Position = 5)]
	[ValidateSet("cmd", "docker")]
	[string]$execType = "cmd"
)

. "$PSScriptRoot\config.ps1"
if (-not ($validModules -contains $module)) {
	throw "Invalid module: $module. Allowed modules: $($validModules -join ', ')"
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$ErrorActionPreference = 'Stop'

# Set service as a variable

$flywayVersion = "10.18.0"
$flywayDir = Join-Path $PSScriptRoot -ChildPath "flyway-$flywayVersion"
$flywayExe = Join-Path $flywayDir -ChildPath "flyway.cmd"
$execPath = Get-Location
# Since development_scripts is parallel to the project folders, use the parent directory
$path = Split-Path $execPath -Parent
$modulePath = Join-Path $execPath -ChildPath "helper" | Join-Path -ChildPath "helper.psm1"
$sampleDataDir = Join-Path $execPath -ChildPath "SampleData" 
Import-Module -Force $modulePath
$dockerNetwork = GetNetworkName

function createDatabase($databaseName, $appsetting) {
	Write-Host "Creating Db $databaseName"
	$sql = " USE master
	if not exists (select * from master.dbo.sysdatabases where name = '$databaseName')
	begin
		CREATE DATABASE [$databaseName] ;
	end ";
	$updatedConnectionString = $appsetting.ConnectionStrings.SqlServer -replace $databaseName, "master"
	$connection = New-Object System.Data.SqlClient.SqlConnection($updatedConnectionString)
	$connection.Open();
	$sqlCommand = New-Object System.Data.SqlClient.SqlCommand
	$sqlCommand.Connection = $connection
	$sqlCommand.CommandText = $sql
	$sqlCommand.ExecuteNonQuery() | Out-Null
}

function getSchemaForModule($module) {
	return $module.ToLower()
}

function runMigration($service, $flywayAction, $createDb) {

	PrintHeader "$service $flywayAction Started"
	$apiPath = Join-Path $path -ChildPath "$service.API" |
	Join-Path -ChildPath "appsettings.Development.json"
	$appsetting = Get-Content $apiPath -Raw | ConvertFrom-Json
	$connString = New-Object System.Data.SqlClient.SQLConnectionStringBuilder( $appsetting.ConnectionStrings.SqlServer)
	$scriptDir = Join-Path $execPath -ChildPath ".." | Join-Path -ChildPath "Database_scripts" | Join-Path -ChildPath $service | Join-Path -ChildPath $module
	$dataDir = Join-Path $sampleDataDir -ChildPath "$service" 

	if ($createDb -and $flywayAction -eq "migrate") {
		createDatabase $connString.InitialCatalog $appsetting
	}
	
	$schema = getSchemaForModule $module
	$flywayURL = "jdbc:sqlserver://sqldata;databaseName=$($connString.InitialCatalog);encrypt=true;trustServerCertificate=true;"
	$flywayArgs = "-outOfOrder=true -ignoreMigrationPatterns=""*:missing"" -baselineOnMigrate=true -placeholderReplacement=false" 
	$flywaySchema = "-schemas=$schema"
	$flywayLogin = " -user=$($connString.UserID) -password=$($connString.Password) -url=""$flywayURL""" 

	if ($flywayAction -eq "clean") {
		$flywayArgs = "$flywayArgs -cleanDisabled=false"
	}

	$options = "$flywayArgs $flywaySchema $flywayLogin $flywayAction"
	RunFlyway $scriptDir $options

	if ($flywayAction -eq "clean") {
		$options = "$flywayArgs $($flywaySchema)HT $flywayLogin $flywayAction"
		RunFlyway $scriptDir $options
	}

	if ($runSampleData -and $flywayAction -eq "migrate") {		
		if (Test-Path $dataDir) {
			PrintHeader "$service SampleData Insertion Started"
			$flywayArgs = "$flywayArgs -validateOnMigrate=false"
			$options = "$flywayArgs $($flywaySchema) $flywayLogin $flywayAction"
			RunFlyway $dataDir $options
		}
	}

	PrintHeader "$service $flywayAction Completed"
}

function ProcessAction($service, $createDatabase) {
	if ($action -eq "cleanAndMigrate") {
		runMigration $service "clean" $createDatabase
		runMigration $service "migrate" $createDatabase
		return
	}
	runMigration $service $action $createDatabase
}

function RunFlyway($location, $options) {
	if ($execType -eq "cmd") {
		$file = Get-Item -Path $location | Select-Object -ExpandProperty FullName
		$options = $options -replace "sqldata", "localhost:$localHosePort"
		invoke-expression " $flywayExe -locations=filesystem:$file $options " 
		if ($LASTEXITCODE -ne 0) {
			exit($LASTEXITCODE);
		}		
	}
	else {
		$command = "docker run --rm --network $dockerNetwork --volume $($location):/flyway/sql flyway/flyway $options"
		Invoke-Expression $command
	}
}


function Install-Flyway() {
	if (Test-Path $flywayExe) {
		Write-Host "Flyway already installed in $flywayDir"
		return 
	}

	# Recreate the directory since it's not complete
	if (Test-Path $flywayDir) {
		Remove-Item -Recurse -Force $flywayDir
	}
    
	New-Item -Type Directory -Path $flywayDir -Force
	

	Write-Host "Downloading Flyway $flywayVersion"
	$flywayUrl = "https://repo1.maven.org/maven2/org/flywaydb/flyway-commandline/$flywayVersion/flyway-commandline-$flywayVersion-windows-x64.zip"
	$flywayZip = "$flywayDir/flywayZip" + $(Split-Path -Path $flywayUrl -Leaf)
	Invoke-WebRequest -Uri $flywayUrl -OutFile $flywayZip

	Write-Host "Flyway zip downloaded"
	
	Expand-Archive -Path $flywayZip -DestinationPath $flywayDir
	Move-Item "$flywayDir/flyway-$flywayVersion/*" $flywayDir
	# Remove-Item $flywayZip   

	Write-Host "Flyway extracted to $flywayDir"
}


if ($execType -eq "cmd") {
	Install-Flyway
}
if ( $module -eq "All" ) {
	# Get actual modules (exclude "All" itself)
	$actualModules = $validModules | Where-Object { $_ -ne "All" }
	Foreach ( $m in $actualModules) {
		# Temporarily set the module variable for the migration
		$originalModule = $module
		$module = $m
		ProcessAction $service $createDatabase
		$module = $originalModule
		$createDatabase = $false;
	}
}
else {
	ProcessAction $service $createDatabase
}
Set-Location $execPath;
