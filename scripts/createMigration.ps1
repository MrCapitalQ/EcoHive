Param(    
    [Parameter(Position = 0, mandatory = $true)]
    [string]
    $name
)

$currentDir = Get-Location

try {
    Set-Location $PSScriptRoot
    Set-Location ..

    dotnet ef migrations add $name --startup-project .\src\MrCapitalQ.EcoHive.Api --project .\src\MrCapitalQ.EcoHive.EcoBee.AspNetCore
}
finally {
    # Return to original directory
    Set-Location $currentDir
}
