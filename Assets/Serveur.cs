using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class Serveur {
    public static Thread zt;
    private static int portServeur;
    private static readonly System.Object l = new System.Object();
    public static int connexionsTraitees = 0;
    public static InfoList logs = new InfoList(10);
    private static FileAttente missions = new FileAttente();
    private const int NB_THREADS_POOL = 5;

    public static int nouveauTraitement() {
        lock(l) {
            return connexionsTraitees++;
        }
    }

    private static void WorkerLoop() {
        while (true) {
            Socket s = missions.Defiler();
            ProtocoleServeur p = new ProtocoleServeur(s);
            p.Protocole();
        }
    }

    public static void Run() {
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try {
            for (int i = 0; i < NB_THREADS_POOL; i++) {
                Thread t = new Thread(new ThreadStart(WorkerLoop));
                t.IsBackground = true;
                t.Start();
            }

            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sock.Bind(new IPEndPoint(IPAddress.Any, portServeur));
            sock.Listen(Client.NB_CL);

            while (true) {
                Socket ear = sock.Accept();
                if (ear != null) {
                    missions.Enfiler(ear);
                }
            }
        }
        catch (Exception) {
            if (sock != null) sock.Close();
            zt = null;
        }
    }

    public static void Demarre(int port) {
        if (zt != null) return;
        portServeur = port;
        zt = new Thread(new ThreadStart(Run));
        zt.Start();
    }
}