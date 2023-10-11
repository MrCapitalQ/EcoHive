$currentDir = Get-Location

try {
    Set-Location $PSScriptRoot
    Set-Location ..

    dotnet ef database update --startup-project .\src\MrCapitalQ.EcoHive.Api --project .\src\MrCapitalQ.EcoHive.EcoBee.AspNetCore
}
finally {
    # Return to original directory
    Set-Location $currentDir
}
