using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Net;
using TMPro;
using System;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;

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

class Serveur {
    public static Thread zt; // Le prof utilise un Thread pour le contrôle
    public const int NB_CNX = 20;
    private static int portServeur;
    private static readonly object l = new object();
    private static int connexionsTraitees = 0;
    public static InfoList logs = new InfoList(10);
    
    private Socket ear;
    public static bool actif = false;

    public Serveur(Socket ear) {
        this.ear = ear;
    }

    public static void ajouteConnexionTraitee() {
        lock(l) { connexionsTraitees++; }
    }

    public static int GetConnexionTraitees() {
        return connexionsTraitees;
    }

    public async Task Protocole() {
        try {
            byte[] buffer = new byte[2048];

            int lus = await ear.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

            if (lus > 0) {
                string msg = Encoding.ASCII.GetString(buffer, 0, lus);
                Debug.Log("Serveur.Protocole(): reçu du client : " + msg);

                int ml = msg.Length;
                string method = msg.Substring(0, 4);
                
                if (method.Equals("POST") && ' ' == msg[4] && '\n' == msg[ml - 1]) {
                    string data = msg.Substring(5, ml - 6);
                    logs.addItem(ear.RemoteEndPoint + " : " + data);
                    
                    Joueur j = Joueur.fromJSON(data);
                    if (null != j) {
                        ajouteConnexionTraitee();
                    }
                }
                byte[] resp = Encoding.ASCII.GetBytes("" + GetConnexionTraitees());
                await ear.SendAsync(new ArraySegment<byte>(resp), SocketFlags.None);
            }
        }
        catch (Exception) { }
        finally {
            ear.Close();
        }
    }

    public static async Task Run() {
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint associationLocale = new IPEndPoint(IPAddress.Any, portServeur);
        
        sock.Bind(associationLocale);
        sock.Listen(NB_CNX);
        actif = true;

        try {
            while (actif) {

                Socket ear = await sock.AcceptAsync();
                Serveur cnx = new Serveur(ear);
                _ = cnx.Protocole(); 
            }
            sock.Close();
        }
        catch (Exception e) {
            sock.Close();
            zt = null;
            throw new Exception("Erreur du serveur :", e);
        }
        Debug.Log("Serveur Fin");
        zt = null;
    }

    public static async void Demarre(int port) {
        portServeur = port;
        zt = Thread.CurrentThread; 
        await Run();
    }

    public static void Stop() {
        actif = false;
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

