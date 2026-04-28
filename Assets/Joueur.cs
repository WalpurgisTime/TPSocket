using UnityEngine;
using System;

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