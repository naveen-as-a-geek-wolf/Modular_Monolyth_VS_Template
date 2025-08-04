Function PrintHeader($header) {
	Write-Host "`n-------------------------- $header ---------------------------------`n"
}

. "$PSScriptRoot\..\config.ps1"

class helper {
	[bool]checkAndSetupScriptFolder($scriptDir) {		
		if (Test-Path $scriptDir) {
			return $true;
		}		
		New-Item -Type Directory -Path $scriptDir -Force
		return $false;
	}

	[bool]checkFileExists($filePath) {
		return Test-Path($filePath);
	}
}

Function GetHelper() {
	return [helper]::new()
}

Function GetNetworkName() {
	$networkList = docker network ls -f "name=$networkName"
	return $networkList.Split(' ') | Where-Object -FilterScript { $_ -match "$networkName" }
}

Export-ModuleMember -Function GetHelper
Export-ModuleMember -Function PrintHeader
Export-ModuleMember -Variable services
Export-ModuleMember -Function GetNetworkName
