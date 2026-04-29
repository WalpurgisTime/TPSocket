using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

// Assurez-vous que les classes InfoList et Joueur existent dans votre projet
public class Serveur {
    public static Thread zt;
    public const int NB_CNX = 20;
    private static int portServeur;
    private static readonly System.Object l = new System.Object();
    private static int connexionsTraitees = 0;

    public static void ajouteConnexionTraitee() {
        lock (l) {
            connexionsTraitees++;
        }
    }

    public static int GetConnexionTraitees() {
        return connexionsTraitees;
    }

    public static InfoList logs = new InfoList(10);

    private Socket ear;

    // Constructeur pour chaque instance de connexion
    public Serveur(Socket ear) {
        this.ear = ear;
    }

    // Logique de traitement de la communication (Protocole)
    public void Protocole() {
        try {
            byte[] buffer = new byte[2048];
            int lus = ear.Receive(buffer);
            
            if (lus > 0) {
                string derniereReception = Encoding.ASCII.GetString(buffer).Substring(0, lus);
                Debug.Log("Serveur.Protocole(): reçu du client : " + derniereReception);

                int ml = derniereReception.Length;
                
                // Vérification basique du format "POST [data] \n"
                if (derniereReception.StartsWith("POST") && derniereReception.EndsWith("\n")) {
                    // Extraction des données (entre "POST " et "\n")
                    string data = derniereReception.Substring(5, ml - 6);
                    logs.addItem(ear.RemoteEndPoint + " : " + data);
                    
                    // Simulation/Appel de la désérialisation du Joueur
                    Joueur j = Joueur.fromJSON(data);
                    if (null != j) {
                        ajouteConnexionTraitee();
                    }
                }
            }

            // Réponse au client : on renvoie le nombre total de connexions traitées
            ear.Send(Encoding.ASCII.GetBytes("" + GetConnexionTraitees()));
        }
        catch (Exception e) {
            Debug.LogError("Erreur dans le protocole : " + e.Message);
        }
        finally {
            ear.Close();
        }
    }

    // Boucle principale du serveur
    public static void Run() {
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress adresse = IPAddress.Any;
        IPEndPoint associationLocale = new IPEndPoint(adresse, portServeur);

        try {
            sock.Bind(associationLocale);
            sock.Listen(NB_CNX);
            Debug.Log("Serveur démarré sur le port " + portServeur);

            while (true) {
                // Bloque ici jusqu'à une nouvelle connexion
                Socket ear = sock.Accept();
                
                // Correction ici : On instancie la classe 'Serveur' elle-même
                Serveur instance = new Serveur(ear);
                Thread t = new Thread(new ThreadStart(instance.Protocole));
                t.Start();
            }
        }
        catch (Exception e) {
            Debug.LogError("Erreur fatale du serveur : " + e.Message);
        }
        finally {
            if (sock != null) sock.Close();
            zt = null;
        }
    }

    // Méthode pour démarrer le serveur depuis un script Unity (ex: Start())
    public static void Demarre(int port) {
        if (null != zt) {
            Debug.Log("Thread serveur déjà démarré");
            return;
        }
        portServeur = port;
        zt = new Thread(new ThreadStart(Run));
        zt.IsBackground = true; // Permet au thread de s'arrêter quand l'application ferme
        zt.Start();
    }
}