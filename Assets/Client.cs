using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

class Client {
    public static int zt = 0;
    private static readonly System.Object l = new System.Object();
    private static System.Random rnd = new System.Random();
    public const int NB_CL = 20;
    private static IPAddress targetAddr;
    private static int portCible;

    public static void Protocole(Socket sock) {
        byte[] buffer = new byte[4096];
        string requeteHTTP = "GET /index.html HTTP/1.1\r\n" +
                             "Host: localhost\r\n" +
                             "Connection: close\r\n\r\n";
                         
        sock.Send(Encoding.ASCII.GetBytes(requeteHTTP));
        
        int lus = sock.Receive(buffer);
        if (lus > 0) {
            string rep = Encoding.ASCII.GetString(buffer, 0, lus);
            MonoBehaviour.print("Client recoit HTTP:\n" + rep);
        }
    }

    public static void Run() {
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try {
            sock.Connect(targetAddr, portCible);
            lock(l) { Thread.Sleep(rnd.Next(10, 50)); }
            Protocole(sock);
        }
        catch (Exception) { }
        finally {
            sock.Close();
            lock(l) { zt--; }
        }
    }

    public static void Demarre(IPAddress target, int port) {
        targetAddr = target;
        portCible = port;
        for (int i = 0; i < NB_CL; i++) {
            new Thread(new ThreadStart(Run)).Start();
            lock(l) { zt++; }
        }
    }
}