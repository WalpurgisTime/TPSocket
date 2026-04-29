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
    private static readonly object l = new object();
    public const int NB_CL = 20;
    private static IPAddress targetAddr;
    private static int portCible;
    private static float mx, my;

    public static void SetMessage(float x, float y) {
        lock (l) { mx = x; my = y; }
    }

    public static void Run() {
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint serverEP = new IPEndPoint(targetAddr, portCible);

        try {
            Joueur j = new Joueur();
            j.id = "Client_" + Thread.CurrentThread.ManagedThreadId;
            lock (l) {
                j.x = mx;
                j.y = my;
                j.timestamp = DateTime.Now.Ticks;
            }

            byte[] data = Encoding.ASCII.GetBytes("POST " + j.toJSON() + "\n");
            sock.SendTo(data, serverEP);
        }
        catch (Exception e) {
            Debug.LogError("Erreur Client UDP: " + e.Message);
        }
        finally {
            sock.Close();
            lock (l) { zt--; }
        }
    }

    public static void Demarre(IPAddress zeTargetID, int port) {
        targetAddr = zeTargetID;
        portCible = port;
        for (int i = 0; i < NB_CL; i++) {
            lock (l) { zt++; }
            new Thread(new ThreadStart(Run)).Start();
        }
    }
}

class Serveur {
    public static Thread zt;
    private static int portServeur;
    private static readonly object l = new object();
    private static int connexionsTraitees = 0;
    public static InfoList logs = new InfoList(10);
    public static bool actif = false;
    private static Dictionary<string, long> derniersTimestamps = new Dictionary<string, long>();

    public static async Task Run() {
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        sock.Bind(new IPEndPoint(IPAddress.Any, portServeur));
        actif = true;
        byte[] buffer = new byte[4096];

        try {
            while (actif) {
                var result = await sock.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, new IPEndPoint(IPAddress.Any, 0));
                string msg = Encoding.ASCII.GetString(buffer, 0, result.ReceivedBytes);
                TraiterMessage(msg, result.RemoteEndPoint);
            }
        }
        catch (Exception e) {
            Debug.LogError("Erreur Serveur: " + e.Message);
        }
        finally {
            sock.Close();
            zt = null;
        }
    }

    private static void TraiterMessage(string msg, EndPoint remoteEP) {
        int ml = msg.Length;
        if (ml > 6 && msg.StartsWith("POST ") && msg.EndsWith("\n")) {
            string data = msg.Substring(5, ml - 6);
            Joueur j = Joueur.fromJSON(data);
            if (j != null) {
                lock (l) {
                    if (!derniersTimestamps.ContainsKey(j.id) || j.timestamp > derniersTimestamps[j.id]) {
                        derniersTimestamps[j.id] = j.timestamp;
                        connexionsTraitees++;
                        logs.addItem(remoteEP.ToString() + " : " + j.x + "," + j.y);
                    }
                }
            }
        }
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
    private string[] list;
    private int pointer = 0;

    public InfoList(int nb) {
        this.list = new string[nb];
    }

    public void addItem(string s) {
        lock (this) {
            this.list[this.pointer] = s;
            this.pointer = (this.pointer + 1) % this.list.Length;
        }
    }

    public string toString() {
        string res = "";
        lock (this) {
            for (int i = 0; i < this.list.Length; i++) {
                string s = this.list[(this.pointer + i) % this.list.Length];
                if (s != null) res += s + "\n";
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
    public long timestamp;

    public String toJSON() {
        return JsonUtility.ToJson(this);
    }
    public static Joueur fromJSON(String json) {
        return JsonUtility.FromJson<Joueur>(json);
    }
}

