using System;
using System.Text;
using System.Net;
using UnityEngine;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

class Serveur {
    public static Thread zt;
    private static TcpListener listener;
    private static bool actif = false;
    private static int portServeur;
    private static readonly object l = new object();
    public static int connexionsTraitees = 0;
    public static InfoList logs = new InfoList(10);

    public static int nouveauTraitement() {
        lock (l) {
            return connexionsTraitees++;
        }
    }

    public static async void Demarre(int port) {
        if (actif) return;
        portServeur = port;
        actif = true;

        try {
            listener = new TcpListener(IPAddress.Any, portServeur);
            listener.Start(Client.NB_CL);

            while (actif) {
                Socket ear = await listener.AcceptSocketAsync();
                _ = ConnexionAsync(ear);
            }
        }
        catch (Exception e) {
            Debug.Log("Serveur arrêté : " + e.Message);
            actif = false;
        }
    }

    private static async Task ConnexionAsync(Socket ear) {
        try {
            byte[] buffer = new byte[2048];
            int lus = await Task.Factory.FromAsync(
                ear.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, null, null),
                ear.EndReceive);

            if (lus > 0) {
                string msg = Encoding.ASCII.GetString(buffer, 0, lus);
                int ml = msg.Length;

                if (msg.StartsWith("POST") && msg[ml - 1] == '\n') {
                    string data = msg.Substring(5, ml - 6);
                    logs.addItem("From: " + ear.RemoteEndPoint + " : " + data);
                    
                    string reponse = "" + nouveauTraitement();
                    byte[] respData = Encoding.ASCII.GetBytes(reponse);
                    
                    await Task.Factory.FromAsync(
                        ear.BeginSend(respData, 0, respData.Length, SocketFlags.None, null, null),
                        ear.EndSend);
                }
            }
        }
        catch (Exception e) {
            Debug.Log("Erreur connexion : " + e.Message);
        }
        finally {
            ear.Close();
        }
    }

    public static void Stop() {
        actif = false;
        listener?.Stop();
    }
}