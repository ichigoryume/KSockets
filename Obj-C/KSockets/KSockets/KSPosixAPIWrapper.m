//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <errno.h>
#include <arpa/inet.h>

#include <netinet/tcp.h>
#include <sys/ioctl.h>

#import "KSPosixAPIWrapper.h"

/*-------------------------------------------------------------------------------
 - local functions
 -------------------------------------------------------------------------------*/
static NSString * convert_sockaddr_to_string(struct sockaddr_in * clientAddr);
static void show_error(const char * funcname, const char * message);


/*-------------------------------------------------------------------------------
 - implementation
 -------------------------------------------------------------------------------*/
// UDPならば、存在しない（であろう）IPに対してコネクションがはれるっぽい？
// 張れてしまえば、そのソケットから自分のIPを調べられる。
NSString * get_device_address()
{
	/* cureate dummy peer address */
	struct sockaddr_in dummy_peer;
	bzero(&dummy_peer, sizeof(dummy_peer));
	dummy_peer.sin_family	= AF_INET;
	dummy_peer.sin_port		= htons(8888);
	inet_pton(AF_INET, "10.0.1.254", &(dummy_peer.sin_addr));
	
	/* create dummy socket */
	int dummy_socket = socket(AF_INET, SOCK_DGRAM, 0);
	if(dummy_socket < 0)
	{
		show_error("get_device_address", "create socket error");
		return nil;
	}
	
	/* connect */
	int ret = connect(dummy_socket, (struct sockaddr *)&dummy_peer, sizeof(dummy_peer));
	if(ret < 0)
	{
		show_error("get_device_address", "connect error");
		return nil;
	}
	
	/* get host address */
	struct sockaddr_in addr;
	socklen_t len = sizeof(addr);
	ret = getsockname(dummy_socket, (struct sockaddr*)&addr, &len);
	if(ret < 0)
	{
		show_error("get_device_address", "getsockname error");
		return nil;
	}
	
	close(dummy_socket);
	
	return [NSString stringWithUTF8String:inet_ntoa(addr.sin_addr)];
}

int create_client_socket()
{
    int ret;
    
    /* create socket */
    int clientSocket = socket(AF_INET, SOCK_STREAM, 0);
    if(clientSocket == -1)
    {
        show_error("create_client_socket", "socket error");
        return -1;
    }
    
	/* don't use send/receive buffer. needs netinet/tcp.h */
    int on = 1;
	ret = setsockopt(clientSocket, IPPROTO_TCP, TCP_NODELAY, &on, sizeof(on));
	if(ret == -1) {
		show_error("create_client_socket", "setsockopt, set TCP_NODELAY error");
		close(clientSocket);
		return -1;
	}
    
    return clientSocket;
}

int connect_to_server(int clientSocket, const char * serverAddress, uint port)
{
    /* create server address */
    struct sockaddr_in serverAddr;
    bzero(&serverAddr, sizeof(struct sockaddr_in));
    serverAddr.sin_port         = htons(port);
    serverAddr.sin_family       = AF_INET;
    serverAddr.sin_addr.s_addr  = inet_addr(serverAddress);
    
    int ret = connect(clientSocket, (struct sockaddr *)&serverAddr, sizeof(serverAddr));
	if(ret == -1) {
		show_error("connect_to_server", "connect error");
		close(clientSocket);
		return -1;
	}
    
    return 0;
}

// 今のところうまく動作しない。
// connectはするっとぬけるが、selectがうまくいかない
int connect_to_server_with_timeout(int clientSocket, const char * serverAddress, uint port)
{
    /* create server address */
    struct sockaddr_in serverAddr;
    bzero(&serverAddr, sizeof(struct sockaddr_in));
    serverAddr.sin_port         = htons(port);
    serverAddr.sin_family       = AF_INET;
    serverAddr.sin_addr.s_addr  = inet_addr(serverAddress);
    
    /* set nonbrocking */
    int val = 1;
    ioctl(clientSocket, FIONBIO, &val);
    
    /* */
    fd_set readfds;
    FD_ZERO(&readfds);
    FD_SET(clientSocket, &readfds);
    
    /* set timeout value */
    struct timeval timeout;
    timeout.tv_sec = 5;
    timeout.tv_usec = 0;
    
    /* connect */
    int ret = connect(clientSocket, (struct sockaddr *)&serverAddr, sizeof(serverAddr));
    if(ret == 0)
    {
        // ノンブロッキングなので、必ずエラーになる　→　成功値がかえってきたらおかしい
        show_error("connect_to_server_with_timeout", "connect error");
        close(clientSocket);
        return -1;
    }
    
    ret = select(clientSocket + 1, &readfds, NULL, NULL, &timeout);
    if(ret == 0)
    {
        show_error("connect_to_server_with_timeout", "connect timeouted");
        return -1;
    }
    
    char rBuff[128];
    ret = (int)recv(clientSocket, rBuff, 0, 0);
    if(ret == -1)
    {
        show_error("connect_to_server_with_timeout", "connect refused");
        return -1;
    }
    
    return 0;
}

int create_listening_socket(uint port)
{
	int ret;
	
	/* create server address */
	struct sockaddr_in serverAddr;
	bzero(&serverAddr, sizeof(struct sockaddr_in));
	serverAddr.sin_port             = htons(port);
	serverAddr.sin_family           = AF_INET;
	serverAddr.sin_addr.s_addr      = htonl(INADDR_ANY);
	
	/* create socket */
	int listeningSocket = 0;
	listeningSocket = socket(AF_INET, SOCK_STREAM, 0);
	if(listeningSocket == -1) {
		show_error("create_listening_socket", "socket error");
		return -1;
	}
	
	/* set reuse option */
	int on = 1;
	ret = setsockopt(listeningSocket, SOL_SOCKET, SO_REUSEADDR, &on, sizeof(on));
	if(ret == -1) {
		show_error("create_listening_socket", "setsockopt, set reuse error");
		close(listeningSocket);
		return -1;
	}
	
	/* don't use send/receive buffer. needs netinet/tcp.h */
	ret = setsockopt(listeningSocket, IPPROTO_TCP, TCP_NODELAY, &on, sizeof(on));
	if(ret == -1) {
		show_error("create_listening_socket", "setsockopt, set TCP_NODELAY error");
		close(listeningSocket);
		return -1;
	}	
	
	/* bind */
	ret = bind(listeningSocket, (struct sockaddr *)&serverAddr, sizeof(serverAddr));
	if(ret == -1) {
		show_error("create_listening_socket", "bind error");
		close(listeningSocket);
		return -1;
	}
    
	/* listen */
	ret = listen(listeningSocket, SOMAXCONN); /* SOMAXCONN = 128 */
	if(ret == -1) {
		show_error("create_listening_socket", "listen error");
		close(listeningSocket);
		return -1;
	}
	
	return listeningSocket;
}

NSString * accept_connection_from_peer(int listeningSocket, int * connectedSocket)
{
	struct sockaddr_in	clientAddr;
	socklen_t			clientAddrSize	= sizeof(clientAddr);
    
	/* accept */
	bzero(&clientAddr, clientAddrSize);
	*connectedSocket = accept(listeningSocket, (struct sockaddr *)&clientAddr, &clientAddrSize);
	if(*connectedSocket == -1) {
		show_error("accept_connection_from_peer", "accept error");
		return nil;
	}
	
	return convert_sockaddr_to_string(&clientAddr);
}

NSString * convert_sockaddr_to_string(struct sockaddr_in * clientAddr)
{
	const char *	retString = NULL;
	char			cString[INET_ADDRSTRLEN];
	
	bzero(cString, INET_ADDRSTRLEN);
	retString = inet_ntop(AF_INET, &(clientAddr->sin_addr), cString, INET_ADDRSTRLEN);
	if(retString == NULL) {
		return @"UNKNOWN";
	}
	
	return [NSString stringWithUTF8String:cString];	
}

int send_n_byte(int sock, void * data, size_t size)
{
    void * ptr          = data;
    size_t leftSize     = size;
    size_t sentSize     = 0;
    
    while(1) {
		
		sentSize = send(sock, ptr, leftSize, 0);
        if(sentSize == -1) {
			show_error("send_n_byte", "send error");
			return -1;
		}
		
		leftSize -= sentSize;
        if(leftSize == 0) {
            break;
		}
		
        ptr += sentSize;
    }
	
	return 0;
}

int receive_n_byte(int sock, void * buf, size_t size)
{
	size_t leftSize		= size;
	size_t receivedSize	= 0;
	char * ptr			= (char *)buf;
	
	while(1) {
		
		receivedSize = recv(sock, ptr, leftSize, 0);
		if(receivedSize == -1) {
			
			if(errno == EINTR) {
				/* シグナルによる割り込みでアボート -> 最受信 */
				continue;
			}
			return -1;
		}
		if(receivedSize == 0) {
			/* ピアがソケットをクローズした。ここでクローズを検出できることはほとんどない？ */
			return 1;
		}
		
		leftSize -= receivedSize;
		if(leftSize == 0) {
			break;
		}
		
		ptr += receivedSize;
	}
	
	return 0;
}

int receive_n_byte_at_most(int sock, void * buf, size_t maxSize)
{
    size_t receivedSize = 0;
    
    while(1)
    {
        receivedSize = recv(sock, buf, maxSize, 0);
        if(receivedSize != -1)
        {
            break;
        }
        if(errno != EINTR) {
            break;
        }

        /* ここまで到達 == シグナルによる割り込みでアボート -> 最受信 */
    }
    
    return (int)receivedSize;
}

void close_socket(int * sock)
{
    if(*sock == 0)
    {
        return;
    }
    
	close(*sock);
	*sock = 0;
}

void show_error(const char * funcname, const char * message)
{
	char errorMessage[32];
    
	strerror_r(errno, errorMessage, 32);
	NSLog(@"%s at %s : %s", message, funcname, errorMessage);
}
