//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

@class KSClientSocket;

@interface KSClientSocketStateMachine : NSObject

- (id)initWithClientSocket:(KSClientSocket *)clientSocket;

- (void)notifyStartRequested;
- (void)notifySendRequested:(NSString *)message;
- (void)notifyCloseRequested;

- (void)notifyConnected;
- (void)notifyReceived;
- (void)notifyDisconnected;

@end
