using System;
using System.Text;
using System.Net.Sockets;
using UnityEngine;

class ProtocoleServeur {
    private Socket ear;
    public ProtocoleServeur(Socket ear) {
        this.ear = ear;
    }

    public void Protocole() {
        byte[] buffer = new byte[2048];
        String reponse = "0";
        try {
            int lus = this.ear.Receive(buffer);
            if (lus > 0) {
                String msg = Encoding.ASCII.GetString(buffer, 0, lus);
                int ml = msg.Length;
                if (msg.StartsWith("POST") && msg[ml - 1] == '\n') {
                    String data = msg.Substring(5, ml - 6);
                    Serveur.logs.addItem("From: " + this.ear.RemoteEndPoint + " : " + data);
                    reponse = "" + Serveur.nouveauTraitement();
                }
                this.ear.Send(Encoding.ASCII.GetBytes(reponse));
            }
            ear.Close();
        }
        catch (Exception e) {
            if (ear != null) ear.Close();
            throw e;
        }
    }
}