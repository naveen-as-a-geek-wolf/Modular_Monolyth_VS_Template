param(
	[Parameter(Mandatory = $true, Position = 0)]
	[string]$fileName,

	[Parameter(Mandatory = $true, Position = 1)]
	[string]$module

)



$ErrorActionPreference = 'Stop'

. "$PSScriptRoot\config.ps1"
if (-not ($validModules -contains $module)) {
	throw "Invalid module: $module. Allowed modules: $($validModules -join ', ')"
}

function NewMigrationScript {

	
	PrintHeader( $("Preparing migration script $fileName in $service "))
	
	$apiPath = Join-Path $path -ChildPath "$service.API"
	$infraPath = Join-Path $path -ChildPath "$service.Infrastructure"; 
	
	$context = "$($module)DbContext"
	$folderPath = Join-Path $infraPath -ChildPath "Modules/$module/Migrations"
	

	Write-Host $folderPath
	Set-Location $infraPath
	dotnet tool install --global dotnet-ef --version $efVersion
	Write-Host "dotnet ef migrations --startup-project $apiPath add $fileName "
	$er = dotnet ef migrations --startup-project $apiPath add $fileName  --context $context -o $folderPath
	if ($er -match "is used by an existing migration" ) {
		throw "Name is already used, Try again with another name"
	}
	PrintHeader( $("Completed migration script $fileName in $service  "))
	Write-Host $er
}

$execPath = Get-Location
$modulePath = Join-Path $execPath -ChildPath "helper" | Join-Path -ChildPath "helper.psm1"
Import-Module -Force $modulePath
# Since development_scripts is parallel to the project folders, use the parent directory
$path = Split-Path $execPath -Parent
$fileName = $fileName -replace "[ |@|!|.]", "-"

NewMigrationScript

Set-Location $execPath;

.\generateSqlScripts.ps1 -autoRemoveDeletedFiles 0 -contextType default -module $module

