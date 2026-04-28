using UnityEngine;

class InfoList {
  private string[] list;
  private int pointer = 0;

  public InfoList(int nb){
    this.list = new string[nb];
  }

  public void addItem(string s){
    this.list[this.pointer] = s;
    this.pointer = (this.pointer + 1) % this.list.Length;
  }

  public string toString(){
    string res = "";
    for(int i = 0; i < this.list.Length; i++){
      string s = this.list[(this.pointer + i) % this.list.Length];
      if(null != s){
        res = res + s + "\n";
      }
    }
    return res;
  }
}