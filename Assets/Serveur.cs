using System;
using System.Text;
using System.Net;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;

class Serveur {
    public static Thread zt;
    private static int portServeur;
    private static readonly System.Object l = new System.Object();
    private static int connexionsTraitees = 0;
    public static InfoList logs = new InfoList(10);

    public static int nouveauTraitement() {
        lock(l) { return connexionsTraitees++; }
    }

    public static void Run() {
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try {
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sock.Bind(new IPEndPoint(IPAddress.Any, portServeur));
            // Augmentation du Listen pour gérer le burst de clients
            sock.Listen(Client.NB_CL);
            while (true) {
                Socket ear = sock.Accept();
                if (ear != null) {
                    new Thread(new ThreadStart(new ProtocoleServeur(ear).Protocole)).Start();
                }
            }
        }
        catch (Exception e) { if (sock != null) sock.Close(); zt = null; throw e; }
        finally { zt = null; }
    }

    public static void Demarre(int port) {
        if (zt != null) return;
        portServeur = port;
        zt = new Thread(new ThreadStart(Run));
        zt.Start();
    }
}