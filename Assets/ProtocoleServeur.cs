using System;
using System.Text;
using System.Net.Sockets;

class ProtocoleServeur {
    private Socket ear;

    public ProtocoleServeur(Socket ear) {
        this.ear = ear;
    }

    public void Protocole() {
        byte[] buffer = new byte[2048];
        try {
            int lus = this.ear.Receive(buffer);
            if (lus > 0) {
                string requete = Encoding.ASCII.GetString(buffer, 0, lus);
                string[] lignes = requete.Split('\n');
                string premiereLigne = lignes[0].Trim();
                
                string[] champs = premiereLigne.Split(' ');

                if (champs.Length >= 3 && champs[0] == "GET") {
                    string chemin = champs[1];
                    Serveur.logs.addItem("HTTP GET: " + chemin);
                    
                    string corps = "<html><body><h1>Serveur HTTP Q9</h1><p>Fichier : " + chemin + "</p></body></html>";
                    string entete = "HTTP/1.1 200 OK\r\n" +
                                    "Content-Type: text/html\r\n" +
                                    "Content-Length: " + Encoding.ASCII.GetByteCount(corps) + "\r\n" +
                                    "Connection: close\r\n\r\n";

                    this.ear.Send(Encoding.ASCII.GetBytes(entete + corps));
                    Serveur.nouveauTraitement();
                }
            }
        }
        catch (Exception) { }
        finally {
            this.ear.Close();
        }
    }
}