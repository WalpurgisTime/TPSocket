using System;
using System.Text;
using System.Net;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;

class Client {
    public static int zt = 0;
    private static readonly System.Object l = new System.Object();
    private static System.Random rnd = new System.Random();
    public const int NB_CL = 20;
    
    private static IPAddress targetAddr;
    private static int portCible;
    private static float mx;
    private static float my;
    private static int compteur = 0;

    public static void SetMessage(float x, float y) {
        lock(l) {
            mx = x; 
            my = y;
        }
    }

    public static String GetMessage(Socket sock) {
        Joueur j = new Joueur();
        j.id = "client_" + (compteur++);
        j.adresseIP = "" + ((IPEndPoint)sock.LocalEndPoint).Address;
        j.port = ((IPEndPoint)sock.LocalEndPoint).Port;
        lock(l) {
            j.x = mx;
            j.y = my;
        }
        return j.toJSON();
    }

    public static void Protocole(Socket sock) {
        byte[] buffer = new byte[2048];
        sock.Send(Encoding.ASCII.GetBytes("POST " + GetMessage(sock) + "\n"));
        int lus = sock.Receive(buffer);
        MonoBehaviour.print("Reçu: " + Encoding.ASCII.GetString(buffer, 0, lus));
    }

    public static void Run() {
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try {
            sock.Connect(targetAddr, portCible);
            lock(l) { Thread.Sleep(rnd.Next(100, 103)); }
            Protocole(sock);
            Thread.Sleep(100);
        }
        catch (Exception e) {
            lock(l) { zt--; }
            sock.Close();
            throw e;
        }
        sock.Close();
        lock(l) { zt--; }
    }

    public static void Demarre(IPAddress zeTargetID, int port) {
        targetAddr = zeTargetID;
        portCible = port;
        for (int i = 0; i < NB_CL; i++) {
            new Thread(new ThreadStart(Run)).Start();
            lock(l) { zt++; }
        }
    }
}