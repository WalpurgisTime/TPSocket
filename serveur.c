/* server.c */
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <string.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>

#define BUFFER_SIZE 1024
#define KNOWN_PORT 9090

void exitOnFail(int result, char *msg, char* cmd){
  char s[BUFFER_SIZE];
  int pid = getpid();
  if(0 > result){
    sprintf(s, "[%d] '%s' : %s", pid, cmd, msg);
    perror(s);
    exit(-1);
  }
}

int main(int argc, char *argv[]) {
    int sock, ear;
    struct sockaddr_in server_addr, client_addr;
    socklen_t ca_size = sizeof(client_addr);
    char s[BUFFER_SIZE];
    int nb_octet;

    // Création du socket
    sock = socket(AF_INET, SOCK_STREAM, 0);
    exitOnFail(sock, "Serveur : Échec création socket", argv[0]);

    // Option pour réutiliser le port immédiatement (SO_REUSEADDR)
    int opt = 1;
    setsockopt(sock, SOL_SOCKET, SO_REUSEADDR, &opt, sizeof(opt));

    server_addr.sin_family = AF_INET;
    server_addr.sin_addr.s_addr = INADDR_ANY;
    server_addr.sin_port = htons(KNOWN_PORT);

    // Association
    exitOnFail(bind(sock, (struct sockaddr *)&server_addr, sizeof(server_addr)), 
               "Serveur : Échec de l'association.", argv[0]);

    // Écoute
    exitOnFail(listen(sock, 1), 
               "Serveur : Échec réservation socket-filles", argv[0]);

    printf("Serveur : En attente sur le port %d...\n", KNOWN_PORT);

    // Acceptation
    ear = accept(sock, (struct sockaddr *)&client_addr, &ca_size);
    exitOnFail(ear, "Serveur : Échec acceptation d'une nouvelle connexion.", argv[0]);

    // Lecture 1
    nb_octet = read(ear, s, BUFFER_SIZE);
    if(0 >= nb_octet){
        perror("Serveur : pas de donnée lisible 1");
        goto fin;
    }
    s[nb_octet] = '\0'; // Sécurité pour l'affichage
    printf("Serveur : connexion depuis %s\n", s);

    // Réponse "Hello"
    sprintf(s, "Hello");
    write(ear, s, strlen(s) + 1);

    // Lecture 2
    nb_octet = read(ear, s, BUFFER_SIZE);
    if(0 >= nb_octet){
        perror("Serveur : pas de donnée lisible 3");
        goto fin;
    }
    s[nb_octet] = '\0';
    printf("Serveur : fin ? %s\n", s);

fin:
    close(ear);
    close(sock);
    printf("Serveur : Fin.\n");
    return 0;
}