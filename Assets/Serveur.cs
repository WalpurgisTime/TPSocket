using System;
using System.Text;
using System.Net;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;

class Serveur {
    public static Thread zt;
    private static int portServeur;
    private static int connexionsTraitees = 0;
    public static string derniereReception = null;

    public static void Protocole(Socket ear) {
        byte[] buffer = new byte[2048];
        int lus = ear.Receive(buffer);
        
        if (lus > 0) {
            derniereReception = Encoding.ASCII.GetString(buffer, 0, lus);
            Debug.Log("Serveur.Protocole(): reçu du client : " + derniereReception);
            
            connexionsTraitees++;
            ear.Send(Encoding.ASCII.GetBytes(connexionsTraitees.ToString()));
        }
    }

    public static void Run() {
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        try {
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, portServeur);
            
            sock.Bind(endPoint);
            sock.Listen(5);

            Socket ear;
            while (true) {
                ear = sock.Accept();
                if (ear != null) {
                    Protocole(ear);
                    ear.Close();
                }
            }
        }
        catch (Exception e) {
            if (sock != null) sock.Close();
            zt = null;
            derniereReception = null;
            throw e;
        }
        finally {
            derniereReception = null;
            zt = null;
        }
    }

    public static void Demarre(int port) {
        if (null != zt) {
            Debug.Log("Thread serveur déjà démarré");
            return;
        }
        portServeur = port;
        zt = new Thread(new ThreadStart(Run));
        zt.Start();
    }
}


  /*
  using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

class Serveur {
  public static void Main(string[] args) {
    byte[] buffer= new byte[2048];
    Socket sock= new Socket(AddressFamily.InterNetwork
                              , SocketType.Stream
                              , ProtocolType.Tcp);
    IPAddress addr= IPAddress.Any;
    IPEndPoint endPoint= new IPEndPoint(addr, 9090);
    sock.Bind(endPoint);
    sock.Listen(1);
    Socket ear= sock.Accept();
    ear.Receive(buffer);
    Console.WriteLine("Server.Main(): Connexion depuis : "
                     + Encoding.ASCII.GetString(buffer));
    ear.Send(Encoding.ASCII.GetBytes("Hello"));
    Array.Clear(buffer, 0, buffer.Length);
    ear.Receive(buffer);
    Console.WriteLine("Server.Main(): Fin ? "
                     + Encoding.ASCII.GetString(buffer));
    ear.Close();
    sock.Close();
    }
    */