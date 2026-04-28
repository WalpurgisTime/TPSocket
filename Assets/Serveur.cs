using System;
using System.Text;
using System.Net;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;

class Serveur {
  public static Thread zt;
  private static int portServeur;
  public static void Run(){
    byte[] buffer = new byte[2048];
    Socket sock = new Socket(AddressFamily.InterNetwork
                              , SocketType.Stream
                              , ProtocolType.Tcp);
    IPAddress addr = IPAddress.Any;
    IPEndPoint endPoint = new IPEndPoint(addr, portServeur);
    sock.Bind(endPoint);
    sock.Listen(1);
    Socket ear = sock.Accept();
    ear.Receive(buffer);
    Debug.Log("Server.Run(): Connexion depuis : "
                     + Encoding.ASCII.GetString(buffer));
    ear.Send(Encoding.ASCII.GetBytes("Hello"));
    Array.Clear(buffer, 0, buffer.Length);
    ear.Receive(buffer);
    Debug.Log("Server.Run(): Fin ? "
                     + Encoding.ASCII.GetString(buffer));
    ear.Close();
    sock.Close();
    Debug.Log("Serveur Fin");
    zt= null;
  }
  public static void Demarre(int port) {
    if(null!=zt){
      Debug.Log("Thread serveur déjà démarré");
      return;
      }
    portServeur= port;
    zt= new Thread(new ThreadStart(Run));
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