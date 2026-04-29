using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Net;
using TMPro;
using System;

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

