using System;
using System.Text;
using System.Net;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;

class Client {
  public static Thread zt;
  private static IPAddress adressseCible;
private static int portCible;
  public static void Run(){
    byte[] buffer = new byte[2048];
    int lus;
    Socket sock = new Socket(AddressFamily.InterNetwork
                              , SocketType.Stream
                              , ProtocolType.Tcp);
    IPAddress addr = IPAddress.Loopback;
    sock.Connect(adressseCible, portCible);
    sock.Send(Encoding.ASCII.GetBytes("Client"));
    lus = sock.Receive(buffer);
    Debug.Log("Client.Run(): reçu du serveur '"
                      + Encoding.ASCII.GetString(buffer)+"'");
    sock.Send(Encoding.ASCII.GetBytes("Fin"));
    Thread.Sleep(10000);
    sock.Close();
    Debug.Log("Client.Run(): Fin");
    zt= null;
    }
  public static void Demarre(IPAddress tgtIP, int port) {
    if(null!=zt){
      Debug.Log("Thread client déjà démarré");
      return;
      }

    adressseCible= tgtIP;
    portCible= port;
    zt= new Thread(new ThreadStart(Run));
    zt.Start();
    }
  }