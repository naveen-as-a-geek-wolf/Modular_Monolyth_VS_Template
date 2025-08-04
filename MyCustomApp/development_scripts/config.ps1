# Configuration file for development scripts
# This file contains shared configuration used across multiple scripts

$projectAssembly = "MyCustomApp-Web"
$efVersion = "9.0.7"
$localHosePort = "1433"

# Default service name
$service = "MyCustomApp"


# Valid modules for validation
$validModules = @("User", "Game", "All")

# Docker network name
$networkName = "nw-mycustomapp"

# Export variables for use in other scripts
$global:service = $service
$global:validModules = $validModules
$global:networkName = $networkName
$global:projectAssembly = $projectAssembly
$global:efVersion = $efVersion
$global:localHosePort = $localHosePort