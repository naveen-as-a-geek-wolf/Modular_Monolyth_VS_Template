
param(
	[Parameter(Position = 0)]
	[ValidateSet($true, $false)]
	[bool]$autoRemoveDeletedFiles = $false,

	[Parameter(Mandatory = $true, Position = 1)]
	[string]$module,

	[Parameter(Position = 2)]
	[ValidateSet("default", "event")]
	[string]$contextType = "default"
)

. "$PSScriptRoot\config.ps1"

if (-not ($validModules -contains $module)) {
	throw "Invalid module: $module. Allowed modules: $($validModules -join ', ')"
}
$toNatural = { [regex]::Replace($_, '\d+', { $args[0].Value.PadLeft(20) }) }
$execPath = Get-Location
# Since development_scripts is parallel to the project folders, use the parent directory
$path = Split-Path $execPath -Parent
$modulePath = Join-Path $execPath -ChildPath "helper" | Join-Path -ChildPath "helper.psm1"
Import-Module -Force $modulePath
$helper = GetHelper 
$ErrorActionPreference = "Stop"

# Set service as a variable


function GenerateSQLScripts($service) {
	PrintHeader( "$service Scripting Started ")
	$servicePath = Join-Path $path -ChildPath "$service.API"
	$scriptDir = Join-Path $execPath -ChildPath ".." | Join-Path -ChildPath "Database_scripts" | Join-Path -ChildPath $service | Join-Path -ChildPath $module
	$efMigrationDir = Join-Path $path -ChildPath "$service.Infrastructure/Modules/$module/Migrations" 
	$context = "$($module)DbContext"

	$processedFiles = @()
	Set-Location $servicePath

	$folderExists = $helper.checkAndSetupScriptFolder($scriptDir)
	# if (!$helper.checkFileExists($efMigrationDir)) {
	# 	Set-Location $execPath
	# 	throw "Migrations are empty in the specified folder $efMigrationDir"
	# }
	
	$efMigrationList = Get-ChildItem $efMigrationDir -Filter *_*.cs | Where-Object -FilterScript {
		$PSItem.Name -notmatch 'Designer' | Sort-Object $ToNatural
	}
	if ($efMigrationList.Length -eq 0) {
		Write-Host $("No migrations has been found in the directory $efMigrationDir ")
		return
	}

	$sqlScripts = Get-ChildItem $scriptDir -Filter V*.sql | ForEach-Object { $_.Name }

	[int]$i = 0;
	foreach ($efMigFile in $efMigrationList) {
		$version, $currentfileName = $efMigFile.BaseName -split '_' , 2
		$normalizedFileName = $currentfileName -replace "[ |@|!|.]", "-"
		$fileName = $("V$($version)__$normalizedFileName.sql")
		$scriptfile = Join-Path $scriptDir -ChildPath $fileName

		Write-Host("Processing $currentfileName")

		if ( $helper.checkFileExists($scriptfile)) {	
			Write-Host("Skipping Already Genereated $currentfileName")
			$i += 1
			$processedFiles += $fileName
			continue
			
		}

		# If ($folderExists -and $helper.checkFileExists($scriptfile)) {
		# Write-Host("Checking for any update to ef script $currentfileName")
		# $existingFile = Get-Item $scriptfile
		# if ($existingFile.LastWriteTimeUtc -gt $efMigFile.LastWriteTimeUtc) {
		# Write-Host("Skipping Already Genereated $currentfileName")
		# $i += 1
		# $processedFiles += $fileName
		# continue
		# }
		# }

		# $undofile = Join-Path $scriptDir -ChildPath $("U$($version)__$normalizedFileName.sql")
		dotnet tool install --global dotnet-ef --version 9.0.0
		if ($i -eq 0) {
			dotnet ef migrations script 0 $currentfileName --output $scriptfile --context $context  | Out-Null
			# dotnet ef migrations script $currentfileName 0 --output $undofile --context $context  | Out-Null
						
			Write-Host "Please verify the generated scripts"
			Invoke-Item $scriptfile
		}
		else {
			$prevVersion, $prevfileName = ($efMigrationList[$i - 1].BaseName) -split '_' , 2  ;
			Write-Host $("dotnet ef migrations script $prevfileName $currentfileName --context $context " )
			dotnet ef migrations script $prevfileName $currentfileName --output $scriptfile --context $context 
			# dotnet ef migrations script $currentfileName $prevfileName --output $undofile --context $context  | Out-Null
			
			Write-Host "Please verify the generated scripts"
			Invoke-Item $scriptfile
		}
		$processedFiles += $fileName
		$i += 1
	}
	PrintHeader( "$service Scripting Completed")
	if ($sqlScripts.Length -eq 0) {
		return
	}
	Write-Host "Checking for Removed Files"
	$removedFiles = Compare-Object -ReferenceObject $processedFiles -DifferenceObject $sqlScripts | Where-Object -FilterScript {
		$PSItem.SideIndicator -notmatch '<='
	}
	if ($removedFiles) {
		if ($autoRemoveDeletedFiles) {
			foreach ($f in $removedFiles) {
				Write-Host "Removing $file"
				$file = $f.InputObject
				Remove-Item  (Join-Path $scriptDir -ChildPath $file)
				# Remove-Item  (Join-Path $scriptDir -ChildPath ($file -replace "^V", "U"))
			}
			return
		}
		Set-Location $execPath;
		throw  "Found existing script files removed from ef migration `n" + ($removedFiles -join "`n")
	}
}

if ( $service -eq "All" ) {
	Foreach ( $s in $services) {
		GenerateSQLScripts $s
	}
}
else {
	GenerateSQLScripts $service
}
Set-Location $execPath;

