. "$PSScriptRoot\config.ps1"
param(
	[Parameter(Position = 0)]
	[ValidateSet("default", "event")]
	[string]$contextType = "default"
)

$toNatural = { [regex]::Replace($_, '\d+', { $args[0].Value.PadLeft(20) }) }
$execPath = Get-Location
# Since development_scripts is parallel to the project folders, use the parent directory
$path = Split-Path $execPath -Parent
$ErrorActionPreference = "Stop"

# Set service as a variable

function checkMigrated($appsetting, $version) {
	$connection = New-Object System.Data.SqlClient.SqlConnection($appsetting.ConnectionStrings.SqlServer)
	$sql = "
			IF OBJECT_ID('[$service].flyway_schema_history', 'U') IS NOT NULL
			BEGIN 
			SELECT 1 FROM [$service].flyway_schema_history fsh 
			WHERE Version = '$version' 
			END 
			";
	$connection.Open();
	$sqlCommand = New-Object System.Data.SqlClient.SqlCommand
	$sqlCommand.Connection = $connection
	$sqlCommand.CommandText = $sql
	return $sqlCommand.ExecuteScalar()
}

function RemoveLastMigration($service) {
	
	$servicePath = Join-Path $path -ChildPath "$service.API";
	$scriptDir = Join-Path $execPath -ChildPath ".." | Join-Path -ChildPath "Database_scripts" | Join-Path -ChildPath $service
	$infraPath = Join-Path $path -ChildPath "$service.Infrastructure"
	$efMigrationDir = Join-Path $infraPath -ChildPath "Migrations"
	$context = "$($service)DbContext"

	$apiPath = Join-Path $path -ChildPath "$service.API" 
	$settingPath = Join-Path  $apiPath  -ChildPath "appsettings.Development.json"
	$appsetting = Get-Content $settingPath -Raw | ConvertFrom-Json

	Set-Location $servicePath

	$efMigrationList = Get-ChildItem $efMigrationDir -Filter *_*.cs | Where-Object -FilterScript {
		$PSItem.Name -notmatch 'Designer' | Sort-Object $ToNatural
	}
	if ($efMigrationList.Length -eq 0) {
		Write-Host $("No migrations has been found in the directory $efMigrationDir ")
		return
	}

	$sqlScripts = Get-ChildItem $scriptDir -Filter V*.sql | ForEach-Object { $_.Name }
	if ($sqlScripts.Length -eq 0) {
		Write-Host $("No SQL has been found in the directory $scriptDir ")
		return
	}
	
	[string]$version
	[string]$migName
	[string]$efMigBaseName 

	if ( $efMigrationList -is [system.array] ) {
		$lastMigFile = $efMigrationList[$efMigrationList.Length - 1]
		$efMigBaseName = $lastMigFile.BaseName
		$version, $migName = $lastMigFile.BaseName -split '_' , 2
	}
	else {
		$efMigBaseName = $efMigrationList.BaseName
		$version, $migName = $efMigrationList.BaseName -split '_' , 2
	}

	$executed = checkMigrated $appsetting $version

	if (!$executed) {
		Remove-Item  (Join-Path $scriptDir -ChildPath $("V$($version)__$migName.sql"))
		dotnet tool install --global dotnet-ef --version $efVersion
		Set-Location $infraPath
		dotnet ef migrations remove  --startup-project $apiPath --context $context

		Write-Host "Succefully Removed $efMigBaseName "
		return
	}
	Set-Location $execPath
	throw "Unable to remove $migName, It's already applied to db, Use Clean in runDevFlyway and then remove "

}
RemoveLastMigration $service
Set-Location $execPath
