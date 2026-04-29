using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

class FileAttente {
    private Queue<Socket> file = new Queue<Socket>();

    public void Enfiler(Socket s) {
        lock(this) {
            file.Enqueue(s);
            Monitor.Pulse(this);
        }
    }

    public Socket Defiler() {
        lock(this) {
            while (file.Count == 0) {
                Monitor.Wait(this);
            }
            return file.Dequeue();
        }
    }
}