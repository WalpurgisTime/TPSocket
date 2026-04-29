using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Serveur {
    public static Thread zt;
    private static int portServeur;
    private static readonly System.Object l = new System.Object();
    public static int connexionsTraitees = 0;
    public static InfoList logs = new InfoList(10);

    public static int nouveauTraitement() {
        lock(l) {
            return connexionsTraitees++;
        }
    }

    public static void Run() {
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        try {
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sock.Bind(new IPEndPoint(IPAddress.Any, portServeur));

            byte[] buffer = new byte[2048];
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

            while (true) {
                int lus = sock.ReceiveFrom(buffer, ref remoteEP);
                if (lus > 0) {
                    string msg = Encoding.ASCII.GetString(buffer, 0, lus);
                    new Thread(() => ProtocoleServeur.TraitementDirect(sock, msg, remoteEP)).Start();
                }
            }
        }
        catch (Exception) {
            zt = null;
        }
        finally {
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