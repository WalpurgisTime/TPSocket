using System;
using System.Text;
using System.Net.Sockets;
using UnityEngine;

class ProtocoleServeur {
    private Socket ear;

    public ProtocoleServeur(Socket ear) {
        this.ear = ear;
    }

    public void Protocole(object o) {
        byte[] buffer = new byte[4096];
        bool keepAlive = true;

        try {
            while (keepAlive) {
                int lus = ear.Receive(buffer);
                if (lus <= 0) break; // Déconnexion du client

                string requete = Encoding.ASCII.GetString(buffer, 0, lus);
                
                // Analyse de l'en-tête pour le Keep-Alive
                if (!requete.Contains("Connection: keep-alive")) {
                    keepAlive = false; 
                }

                // Préparation du contenu (le corps)
                string corps = "<html><body><h1>Serveur Unity HTTP</h1><p>Connexions: " 
                            + GetConnexionTraitees() + "</p></body></html>";
                byte[] corpsBytes = Encoding.UTF8.GetBytes(corps);

                // Construction de l'en-tête de réponse
                StringBuilder reponse = new StringBuilder();
                reponse.Append("HTTP/1.1 200 OK\r\n");
                reponse.Append("Date: " + DateTime.Now.ToUniversalTime().ToString("r") + "\r\n");
                reponse.Append("Server: UnityMiniServer/1.0\r\n");
                reponse.Append("Content-Type: text/html; charset=UTF-8\r\n");
                reponse.Append("Content-Length: " + corpsBytes.Length + "\r\n");
                reponse.Append("Connection: " + (keepAlive ? "keep-alive" : "close") + "\r\n");
                reponse.Append("\r\n"); // Ligne vide cruciale

                // Envoi de l'en-tête puis du corps
                ear.Send(Encoding.ASCII.GetBytes(reponse.ToString()));
                ear.Send(corpsBytes);

                ajouteConnexionTraitee();

                // Si on ne garde pas la connexion, on sort de la boucle
                if (!keepAlive) break;
            }
        }
        catch (Exception e) {
            Debug.Log("Erreur Protocole HTTP: " + e.Message);
        }
        finally {
            ear.Close();
        }
    }
}