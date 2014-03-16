//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//


/*-------------------------------------------------------------------------------
 - posix socket関連のAPIのラッパー。
 - 利用側がsocket関連のヘッダをインクルードしなくて済む程度に隠蔽。
 -------------------------------------------------------------------------------*/


// ローカルアドレス（端末のアドレス）を取得する
NSString * get_device_address();


// クライアント用のソケットを生成する
int create_client_socket();

// 生成したクライアントソケットを使ってサーバへ接続する
int connect_to_server(int clientSocket, const char * serverAddress, uint port);
int connect_to_server_with_timeout(int clientSocket, const char * serverAddress, uint port);


// リッスン用のソケットを作成する(bindしてlisten)
int create_listening_socket(uint port);

// クライアントからの接続を受け付ける。引数で接続したソケットのディスクリプタをもらう。
// 戻り値はクライアントのアドレス
NSString *	accept_connection_from_peer(int listeningSocket, int * connectedSocket);


// 指定したサイズのデータを送信する。
int	send_n_byte(int sock, void * data, size_t size);


// 指定したサイズのデータを受信する
// 受信が完了するまではブロック。
// 戻り値 0:正常終了 1:ピアのソケットクローズを検出 -1:エラー
int	receive_n_byte(int sock, void * buf, size_t size);


// 多くとも maxSize を超えないデータを受信する
// 受信が完了するまでブロックし、1byteでも受信したら終了。
// 戻り値 0:正常終了 1:ピアのソケットクローズを検出 -1:エラー
int receive_n_byte_at_most(int sock, void * buf, size_t maxSize);


// ソケットをクローズする
void close_socket(int * sock);
