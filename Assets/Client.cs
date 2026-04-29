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

    public static String GetMessage(EndPoint localEP) {
        Joueur j = new Joueur();
        j.id = "client_" + (compteur++);
        j.adresseIP = "" + ((IPEndPoint)localEP).Address;
        j.port = ((IPEndPoint)localEP).Port;
        lock(l) {
            j.x = mx;
            j.y = my;
        }
        return j.toJSON();
    }

    public static void Run() {
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        try {
            EndPoint targetEP = new IPEndPoint(targetAddr, portCible);
            lock(l) { Thread.Sleep(rnd.Next(100, 103)); }
            
            byte[] buffer = new byte[2048];
            string jsonMsg = GetMessage(new IPEndPoint(IPAddress.Any, 0));
            byte[] data = Encoding.ASCII.GetBytes("POST " + jsonMsg + "\n");
            
            sock.SendTo(data, targetEP);

            sock.ReceiveTimeout = 800;
            try {
                int lus = sock.Receive(buffer);
                MonoBehaviour.print("UDP Reçu: " + Encoding.ASCII.GetString(buffer, 0, lus));
            } catch (SocketException) {
                MonoBehaviour.print("Paquet obsolète : délai de réponse dépassé.");
            }
        }
        catch (Exception e) {
            Debug.LogError("Erreur Client UDP: " + e.Message);
        }
        finally {
            sock.Close();
            lock(l) { zt--; }
        }
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