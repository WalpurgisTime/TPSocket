using System;

class InfoList {
    private String[] list;
    private int pointer = 0;

    public InfoList(int nb) {
        this.list = new String[nb];
    }

    public void addItem(String s) {
        lock (this) {
            this.list[this.pointer] = s;
            this.pointer = (this.pointer + 1) % this.list.Length;
        }
    }

    public String toString() {
        String res = "";
        lock (this) {
            for (int i = 0; i < this.list.Length; i++) {
                String s = this.list[(this.pointer + i) % this.list.Length];
                if (s != null) {
                    res = res + s + "\n";
                }
            }
        }
        return res;
    }
}