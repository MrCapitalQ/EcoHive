Set-Location $PSScriptRoot
Set-Location ..\..

dotnet ef database update --startup-project .\src\MrCapitalQ.EcoHive.Api --project .\src\MrCapitalQ.EcoHive.EcoBee.AspNetCore
