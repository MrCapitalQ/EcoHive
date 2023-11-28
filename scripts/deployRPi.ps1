$publishDirectory = ".\src\MrCapitalQ.EcoHive.Api\bin\Release\net8.0\linux-arm64\publish"
$deployDirectory = "/var/www/EcoHive"
$sshUser = "q"
$sshHostName = "raspberrypi"

$sshDeployTarget = "$sshUser@$sshHostName"
$currentDir = Get-Location

try {
    Set-Location $PSScriptRoot
    Set-Location ..

    # Clean publish directory
    Remove-Item -LiteralPath $publishDirectory -Force -Recurse -ErrorAction Ignore

    # Build and publish app for Raspberry Pi OS 64 bit.
    dotnet publish .\src\MrCapitalQ.EcoHive.Api --configuration Release --runtime linux-arm64 --self-contained true
    if (!$?) { Exit $LASTEXITCODE }

    # Create EF Core migration bundle for Raspberry Pi OS 64 bit.
    dotnet ef migrations bundle --output $publishDirectory\efbundle --startup-project .\src\MrCapitalQ.EcoHive.Api --project .\src\MrCapitalQ.EcoHive.EcoBee.AspNetCore --configuration Release --target-runtime linux-arm64 --self-contained --force
    if (!$?) { Exit $LASTEXITCODE }

    # Include service definition file in publish directory.
    Copy-Item .\infra\linux\kestrel-ecohive.service $publishDirectory

    # Include service definition file in publish directory.
    Copy-Item .\infra\linux\configure.sh $publishDirectory

    # Stop existing sevice
    ssh $sshDeployTarget "sudo systemctl stop kestrel-ecohive.service"
    
    # Ensure deploy destination exists
    ssh $sshDeployTarget "sudo mkdir -p $deployDirectory && sudo chown $sshUser $deployDirectory"
    if (!$?) { Exit $LASTEXITCODE }

    # Copy publish directory to Raspberry Pi deploy destination
    scp -r $publishDirectory\* "$($sshDeployTarget):$deployDirectory"
    if (!$?) { Exit $LASTEXITCODE }

    # Run script to configure service
    ssh $sshDeployTarget "bash $deployDirectory/configure.sh"
}
finally {
    # Return to original directory
    Set-Location $currentDir
}
