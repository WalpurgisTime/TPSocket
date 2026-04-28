using System;
using System.Text;
using System.Net;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;

class Client {
    public static Thread zt;
    private static IPAddress targetAddr;
    private static int portCible;
    private static int compteur = 0;
    private static float mx;
    private static float my;

    public static void SetMessage(float x, float y) {
        mx = x;
        my = y;
    }

    public static String GetMessage(Socket sock) {
        Joueur j = new Joueur();
        j.id = "client_" + (compteur++);
        j.adresseIP = "" + ((IPEndPoint)sock.LocalEndPoint).Address;
        j.port = ((IPEndPoint)sock.LocalEndPoint).Port;
        j.x = mx;
        j.y = my;
        return j.toJSON();
    }

    public static void Protocole(Socket sock) {
        int lus;
        byte[] buffer = new byte[2048];
        sock.Send(Encoding.ASCII.GetBytes("POST " + GetMessage(sock) + "\n"));
        lus = sock.Receive(buffer);
        MonoBehaviour.print("Client.Protocole(): reçu du serveur '"
                          + Encoding.ASCII.GetString(buffer, 0, lus)
                          + "' connexions traitées");
    }

    public static void Run() {
        Socket sock = new Socket(AddressFamily.InterNetwork
                                  , SocketType.Stream
                                  , ProtocolType.Tcp);
        try {
            sock.Connect(targetAddr, portCible);
            Protocole(sock);
        }
        catch (Exception e) {
            zt = null;
            sock.Close();
            throw e;
        }
        sock.Close();
        MonoBehaviour.print("Client.Run(): Fin");
        zt = null;
    }

    public static void Demarre(IPAddress tgtIP, int port) {
        if (null != zt) {
            Debug.Log("Thread client déjà démarré");
            return;
        }
        targetAddr = tgtIP;
        portCible = port;
        zt = new Thread(new ThreadStart(Run));
        zt.Start();
    }
}