[CmdletBinding()]
param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]] $CliArguments
)

$project = Join-Path $PSScriptRoot "..\src\MiniAV.Core\MiniAV.Core.csproj"
dotnet run --project $project -- @CliArguments
exit $LASTEXITCODE
