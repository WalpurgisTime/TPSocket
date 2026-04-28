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
  private static String message;

  public static void SetMessage(float x, float y){
    message = "{ \"x\": " + x + ", \"y\": " + y + " }";
  }

  public static void Protocole(Socket sock){
    int lus;
    byte[] buffer = new byte[2048];
    sock.Send(Encoding.ASCII.GetBytes("POST " + message + "\n"));
    lus = sock.Receive(buffer);
    MonoBehaviour.print("Client.Protocole(): reçu du serveur '"
                      + Encoding.ASCII.GetString(buffer, 0, lus)
                      + "' connexions traitées");
  }

  public static void Run(){
    Socket sock = new Socket(AddressFamily.InterNetwork
                              , SocketType.Stream
                              , ProtocolType.Tcp);
    try{
      sock.Connect(targetAddr, portCible);
      Protocole(sock);
    }
    catch(Exception e){
      zt = null;
      sock.Close();
      throw e;
    }
    sock.Close();
    MonoBehaviour.print("Client.Run(): Fin");
    zt = null;
  }

  public static void Demarre(IPAddress tgtIP, int port) {
    if(null != zt){
      Debug.Log("Thread client déjà démarré");
      return;
    }

    targetAddr = tgtIP;
    portCible = port;
    zt = new Thread(new ThreadStart(Run));
    zt.Start();
  }
}