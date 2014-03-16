//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import "KSClientSocket.h"

#import "KSISender.h"
#import "KSVariableSizeMessageSender.h"
#import "KSFixedSizeMessageSender.h"
#import "KSTerminatedMessageSender.h"

#import "KSIReceiver.h"
#import "KSVariableSizeMessageReceiver.h"
#import "KSFixedSizeMessageReceiver.h"
#import "KSTerminatedMessageReceiver.h"

#import "KSReceiveTask.h"
#import "KSConnectTask.h"

#import "KSClientSocketStateMachine.h"
#import "KSPosixAPIWrapper.h"


@interface KSClientSocket ()

// 送信と受信
@property id<KSISender>     sender;
@property id<KSIReceiver>   receiver;   // レシーバーっていうか、ペイロードからメッセージを復元する人。名称変更必要か？

// ソケットとその非同期処理を行うクラス
@property int               socket;
@property KSConnectTask *   connectTask;
@property KSReceiveTask *   receiveTask;

// 状態遷移
@property KSClientSocketStateMachine * stateMachine;


@end


@implementation KSClientSocket

/*-------------------------------------------------------------------------------
 - initializing methods
 -------------------------------------------------------------------------------*/
- (id)initVariableSizeSocket:(NSString *)serverAddress serverPort:(uint)port
{
    self = [super init];

    self.sender     = [[KSVariableSizeMessageSender alloc] init];
    self.receiver   = [[KSVariableSizeMessageReceiver alloc] init];
    [self initializeProperties:serverAddress port:port];
    
    return self;
}

- (id)initFixedSizeSocket:(NSString *)serverAddress serverPort:(uint)port size:(int)size
{
    self = [super init];
    
    self.sender     = [[KSFixedSizeMessageSender alloc] initWithPayloadSize:size];
    self.receiver   = [[KSFixedSizeMessageReceiver alloc] initWithPayloadSize:size];
    [self initializeProperties:serverAddress port:port];

    return self;
}

- (id)initTerminatedMessageSocket:(NSString *)serverAddress serverPort:(uint)port terminator:(NSString *)terminator
{
    self = [super init];
    
    self.sender     = [[KSTerminatedMessageSender alloc] initWithTerminator:terminator];
    self.receiver   = [[KSTerminatedMessageReceiver alloc] initWithTerminator:terminator];
    [self initializeProperties:serverAddress port:port];

    return self;
}

- (void)initializeProperties:(NSString *)serverAddress port:(uint)port;
{
    self.connected      = [[KSEvent alloc] init];
    self.disconnected   = [[KSEvent alloc] init];
    self.received       = [[KSEvent alloc] init];
    self.error          = [[KSEvent alloc] init];
    
    self.localAddress   = get_device_address();
    self.serverAddress  = serverAddress;
    self.serverPort     = port;
    
    self.stateMachine   = [[KSClientSocketStateMachine alloc] initWithClientSocket:self];
}


/*-------------------------------------------------------------------------------
 - user requests
 -------------------------------------------------------------------------------*/
- (void)start
{
    [self.stateMachine notifyStartRequested];
}

- (void)close
{
    [self.stateMachine notifyCloseRequested];
}

- (void)send:(NSString *)message
{
    [self.stateMachine notifySendRequested:message];
}


/*-------------------------------------------------------------------------------
 - event handlers of tasks
 -------------------------------------------------------------------------------*/

- (void)connectedHandler:(id)object
{
    [self.stateMachine notifyConnected];
}

- (void)receivedHandler:(id)object
{
    [self.stateMachine notifyReceived];
}

- (void)disconnectedHandler:(id)object
{
    [self.stateMachine notifyDisconnected];
}


/*-------------------------------------------------------------------------------
 - methods for state classes
 -------------------------------------------------------------------------------*/

- (void)startConnect
{
    self.connectTask = [[KSConnectTask alloc] initWithServerAddress:self.serverAddress port:self.serverPort];

    [self.connectTask.connected addEventListener:self selector:@selector(connectedHandler:)];
    [self.connectTask run];
}

- (void)startReceive
{
    self.socket = self.connectTask.clientSocket;
    self.receiveTask = [[KSReceiveTask alloc] initWithSocket:self.socket];

    [self.receiveTask.received addEventListener:self selector:@selector(receivedHandler:)];
    [self.receiveTask.socketClosed addEventListener:self selector:@selector(disconnectedHandler:)];
    [self.receiveTask run];
}

- (void)cleanup
{
    if(self.receiveTask != nil)
    {
        [self.receiveTask.received removeEventListener:self selector:@selector(receivedHandler:)];
        [self.receiveTask.socketClosed removeEventListener:self selector:@selector(disconnectedHandler:)];
        [self.receiveTask stop];
        self.receiveTask = nil;
    }
    if(self.connectTask != nil)
    {
        [self.connectTask.connected removeEventListener:self selector:@selector(connectedHandler:)];
        [self.connectTask stop];
    }
    
    close_socket(&_socket);
}

- (void)sendMessage:(NSString *)message
{
    [self.sender sendMessage:message socket:self.socket];
}

- (void)dispatchReceivedMessages
{
    NSArray * messages = [self.receiver push:self.receiveTask.buffer length:self.receiveTask.receivedSize];
    for (NSString * message in messages)
    {
        [self.received dispatchEventWithObject:message waitUntilDone:NO];
    }
}

- (void)emitConnected
{
    [self.connected dispatchEventWithObject:self waitUntilDone:NO];
}

- (void)emitDisconnected
{
    [self.disconnected dispatchEventWithObject:self waitUntilDone:NO];
}

- (void)emitReceived:(NSString *)message
{
    [self.received dispatchEventWithObject:message waitUntilDone:NO];
}

- (void)emitError:(NSString *)message
{
    [self.error dispatchEventWithObject:message waitUntilDone:YES];
}


@end
