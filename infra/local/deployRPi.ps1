Set-Location $PSScriptRoot
Set-Location ..\..

$publishDirectory = ".\src\MrCapitalQ.EcoHive.Api\bin\Release\net7.0\linux-arm64\publish"

# Clean publish directory
Remove-Item -LiteralPath $publishDirectory -Force -Recurse -ErrorAction Ignore

# Build and publish app for Raspberry Pi OS 64 bit.
dotnet publish .\src\MrCapitalQ.EcoHive.Api --configuration Release --runtime linux-arm64 --self-contained

# Create EF Core migration bundle for Raspberry Pi OS 64 bit.
dotnet ef migrations bundle --output $publishDirectory\efbundle --startup-project .\src\MrCapitalQ.EcoHive.Api --project .\src\MrCapitalQ.EcoHive.EcoBee.AspNetCore --configuration Release --target-runtime linux-arm64 --self-contained --force

# Include service definition file in publish directory.
Copy-Item .\infra\linux\kestrel-ecohive.service $publishDirectory

# Include service definition file in publish directory.
Copy-Item .\infra\linux\configure.sh $publishDirectory

# Copy publish directory to Raspberry Pi deploy destination
scp -r $publishDirectory\* q@raspberrypi:/var/www/EcoHive

# SSH into Raspberry Pi and execute the following command
# sudo bash /var/www/EcoHive/configure.sh
Write-Host "Build and file transfer complete." -ForegroundColor green
Write-Host "To complete the deployment, SSH into the Raspberry Pi and execute the following:" -ForegroundColor green
Write-Host "    sudo bash /var/www/EcoHive/configure.sh" -ForegroundColor yellow