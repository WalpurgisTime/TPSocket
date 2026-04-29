
using UnityEngine ;
using UnityEngine.UI; 
using System.Net;
using TMPro;
using System;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public class TPReseauScript : MonoBehaviour
{
    public Button ServeurBtn,ClientBtn;
    public TMP_InputField adresseCibleIF, portServeurIF;
    public TMP_InputField sourisXIF, sourisYIF;
    public TMP_Text receptionServeur;

    private IPAddress adresseCibleChaine= IPAddress.Parse("127.0.0.1");
    private int portServeur= 9090;
    void Start()
    {
        ServeurBtn.onClick.AddListener(ClicServeurDemarre);
        ClientBtn.onClick.AddListener(ClicCLientDemarre);
        adresseCibleIF.onEndEdit.AddListener(recupereAdresseCible);
        portServeurIF.onEndEdit.AddListener(recuperePortServeur);
        Client.SetMessage(0,0);

    }

    public void ClicServeurDemarre(){
        Debug.Log("Serveur should start");
        Serveur.Demarre(portServeur);
     }

     public void ClicCLientDemarre(){
        Debug.Log("Client should start");
        Client.Demarre(adresseCibleChaine, portServeur);
    }

    public void recuperePortServeur(String s){
    portServeur= int.Parse(s);
    Debug.Log("p='"+s+"'");
    }
    public void recupereAdresseCible(String s){
    adresseCibleChaine= IPAddress.Parse(s);
    Debug.Log("s='"+s+"'");
    }

    void Update()
    {
        ServeurBtn.interactable= (null==Serveur.zt);
        //ClientBtn.interactable= (null==Client.zt);
        ClientBtn.interactable = (0 == Client.zt);
        Vector3 pos= Input.mousePosition;
        sourisXIF.text= ""+pos.x;
        sourisYIF.text= ""+pos.y;
        Client.SetMessage(pos.x, pos.y);
        receptionServeur.text = Serveur.logs.toString();
    }

}

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
        string requete = "GET /index.html HTTP/1.1\r\nHost: localhost\r\n\r\n";
        sock.Send(Encoding.ASCII.GetBytes(requete));
        
        int lus = sock.Receive(buffer);
        if (lus > 0) {
            MonoBehaviour.print("Client reçu: " + Encoding.ASCII.GetString(buffer, 0, lus));
        }
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

class Serveur {
    public static Thread zt;
    private static int portServeur;
    private static readonly System.Object l = new System.Object();
    public static int connexionsTraitees = 0;
    public static InfoList logs = new InfoList(10);
    private static FileAttente missions = new FileAttente();
    private const int NB_THREADS_POOL = 5;

    public static int nouveauTraitement() {
        lock(l) {
            return connexionsTraitees++;
        }
    }

    private static void WorkerLoop() {
        while (true) {
            Socket s = missions.Defiler();
            ProtocoleServeur p = new ProtocoleServeur(s);
            p.Protocole();
        }
    }

    public static void Run() {
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try {
            for (int i = 0; i < NB_THREADS_POOL; i++) {
                Thread t = new Thread(new ThreadStart(WorkerLoop));
                t.IsBackground = true;
                t.Start();
            }

            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sock.Bind(new IPEndPoint(IPAddress.Any, portServeur));
            sock.Listen(Client.NB_CL);

            while (true) {
                Socket ear = sock.Accept();
                if (ear != null) {
                    missions.Enfiler(ear);
                }
            }
        }
        catch (Exception e) {
            if (sock != null) sock.Close();
            zt = null;
            throw e;
        }
        finally {
            zt = null;
        }
    }

    public static void Demarre(int port) {
        if (zt != null) return;
        portServeur = port;
        zt = new Thread(new ThreadStart(Run));
        zt.Start();
    }
}

class InfoList {
    private String[] list;
    private int pointer = 0;

    public InfoList(int nb) {
        this.list = new String[nb];
    }

    public void addItem(String s) {
        lock (this) {
            this.list[this.pointer] = s;
            this.pointer = (this.pointer + 1) % this.list.Length;
        }
    }

    public String toString() {
        String res = "";
        lock (this) {
            for (int i = 0; i < this.list.Length; i++) {
                String s = this.list[(this.pointer + i) % this.list.Length];
                if (s != null) {
                    res = res + s + "\n";
                }
            }
        }
        return res;
    }
}

class FileAttente {
    private Queue<Socket> file = new Queue<Socket>();

    public void Enfiler(Socket s) {
        lock(this) {
            file.Enqueue(s);
            Monitor.Pulse(this);
        }
    }

    public Socket Defiler() {
        lock(this) {
            while (file.Count == 0) {
                Monitor.Wait(this);
            }
            return file.Dequeue();
        }
    }
}

[Serializable]
public class Joueur {
    public String id;
    public String adresseIP;
    public int port;
    public float x;
    public float y;

    public String toJSON() {
        return JsonUtility.ToJson(this);
    }
    public static Joueur fromJSON(String json) {
        return JsonUtility.FromJson<Joueur>(json);
    }
}

class ProtocoleServeur {
    private Socket ear;
    public ProtocoleServeur(Socket ear) {
        this.ear = ear;
    }

    public void Protocole() {
        byte[] buffer = new byte[4096];
        bool keepalive = true;

        try {
            while (keepalive) {
                int lus = this.ear.Receive(buffer);
                if (lus <= 0) break;

                string msg = Encoding.ASCII.GetString(buffer, 0, lus);
                string[] lignes = msg.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                
                if (lignes.Length > 0) {
                    string[] parts = lignes[0].Split(' ');
                    if (parts.Length >= 2 && parts[0] == "GET") {
                        string chemin = parts[1] == "/" ? "index.html" : parts[1].TrimStart('/');
                        
                        keepalive = msg.Contains("Connection: keep-alive");

                        string corps;
                        string status = "200 OK";

                        if (System.IO.File.Exists(chemin)) {
                            corps = System.IO.File.ReadAllText(chemin);
                        } else {
                            status = "404 Not Found";
                            corps = "<html><body><h1>404 - Fichier non trouve</h1></body></html>";

                        string date = DateTime.Now.ToString("R"); 
                        string entete = "HTTP/1.1 " + status + "\r\n" +
                                       "Date: " + date + "\r\n" +
                                       "Server: MiniServeurUnity/1.0\r\n" +
                                       "Content-Type: text/html\r\n" +
                                       "Content-Length: " + Encoding.UTF8.GetByteCount(corps) + "\r\n" +
                                       "Connection: " + (keepalive ? "keep-alive" : "close") + "\r\n" +
                                       "\r\n";

                        this.ear.Send(Encoding.ASCII.GetBytes(entete + corps));
    
                        if (!keepalive) break;
                    }
                }
            }
        }
        catch (Exception e) {
            Debug.Log("Erreur Protocole: " + e.Message);
        }
        finally {
            ear.Close();
        }
    }
}