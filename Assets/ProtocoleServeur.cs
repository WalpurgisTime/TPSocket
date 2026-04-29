using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

class ProtocoleServeur {
    public static void TraitementDirect(Socket mainSock, String msg, EndPoint remoteEP) {
        try {
            int ml = msg.Length;
            if (msg.StartsWith("POST") && msg[ml - 1] == '\n') {
                String data = msg.Substring(5, ml - 6);
                Serveur.logs.addItem("UDP From " + remoteEP + " : " + data);
                
                String reponse = "" + Serveur.nouveauTraitement();
                mainSock.SendTo(Encoding.ASCII.GetBytes(reponse), remoteEP);
            }
        }
        catch (Exception) { }
    }
}