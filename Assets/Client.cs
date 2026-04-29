using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

class Client {
  public static int zt= 0;
  private static System.Random rnd= new System.Random();
/*
Pour minimiser les blocages entre les threads, on va utiliser des
verrous différents en fonction des ressources accédées. l_pos
permettra de garder la mise à jour des champs mx et my et surtout
d'éviter la concurrence entre les threads voulant incrémenter le
compteur de joueur pour créer un nouveau nom.
*/
  private static readonly System.Object l_pos= new System.Object();
/*
l_zt gardera un accès exclusif au compteur de clients zt.
*/
  private static readonly System.Object l_zt= new System.Object();
  public const int NB_CL= 20;
  private static IPAddress adressseCible;
  private static int portCible;
  private static int compteur= 0;
  private static float mx;
  private static float my;
  private Socket sock;
  private Client(){
    }
  public static void SetMessage(float x, float y){
    lock(l_pos){
      mx= x;
      my= y;
      }
    }
  public String GetMessage(){
    Joueur j= new Joueur();
    j.adresseIP= ""+((IPEndPoint)sock.LocalEndPoint).Address;
    j.port= ((IPEndPoint)sock.LocalEndPoint).Port;
    lock(l_pos){
      j.id= "client_"+(compteur++);
      j.x= mx;
      j.y= my;
    }
    return j.toJSON();
    }
  public void Protocole(){
    int lus;
    byte[] buffer= new byte[2048];
    sock.Send(Encoding.ASCII.GetBytes("POST "+GetMessage()+"\n"));
    lus= sock.Receive(buffer);
    MonoBehaviour.print("Client.Protocole(): reçu du serveur '"
                      + Encoding.ASCII.GetString(buffer)
                      +"' connexions traitées");
    }
  public void Run(object o){
    sock= new Socket(AddressFamily.InterNetwork
                              , SocketType.Stream
                              , ProtocolType.Tcp);
    try{
      sock.Connect(adressseCible, portCible);
      int tempo= 100;
/*
On utilise l'objet rnd lui même pour se prémunir contre une
utilisation concurrente de l'objet. En effet, l'objet Random de C#
n'est pas «thread safe» et une utilisation concurrente peu conduire à
récupérer des 0 systématiquement. En utilisant des verrous différents
pour des ressources différentes, on limite les ralentissement tout en
préservant la cohérence de la mémoire.
*/
      lock(rnd){
        tempo= rnd.Next(100, 103);
        }
      Thread.Sleep(tempo);
      Protocole();
      Thread.Sleep(100);
      }
    catch(Exception e){
      lock(l_zt){
        zt--;
        }
      sock.Close();
      throw new Exception("Erreur du client :", e);
      }
    sock.Close();
    MonoBehaviour.print("Client.Run(): Fin");
    lock(l_zt){
      zt--;
      }
    }
  public static void Demarre(IPAddress tgtIP, int port) {
    adressseCible= tgtIP;
    portCible= port;
    for(int i= 0; i<NB_CL; i++){
/*
Plutôt que d'instancier les threads une à une, on va utiliser le pool
de thread de C# qui va optimiser tout cela. On crée un WorkItem par
client et on l'ajoute à la file d'attente des travaux à faire.
Le système de pool va alors répartir efficacement les tâche entre
différents thread dédié à cela (les workers). On a donc pas besoin de
créer les threads et de gérer la synchronisation, etc. Le système se
charge de tout ça au mieux pour nous et on a juste à se concentrer sur
la gestion d'une communication.
*/
      ThreadPool.QueueUserWorkItem(new Client().Run);
      lock(l_zt){
        zt++;
        }
      }
    }
  }