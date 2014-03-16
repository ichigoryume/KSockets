//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import "KSClientSocketStateMachine.h"
#import "KSClientSocket_Internal.h"

/*-------------------------------------------------------------------------------
 - extension of NSMutableDictionary
 -------------------------------------------------------------------------------*/
@interface NSMutableDictionary (KSocketExtension)
- (void)setKey:(id)key value:(id)value;
@end

@implementation NSMutableDictionary (KSocketExtension)

- (void)setKey:(NSString *)key value:(id)value
{
    [self setValue:value forKey:key];
}

@end



/*-------------------------------------------------------------------------------
 - Implementation of State Machine
 -------------------------------------------------------------------------------*/

@interface KSClientSocketStateMachine ()

@property KSClientSocket *      clientSocket;
@property NSMutableDictionary * stateIdle;
@property NSMutableDictionary * stateConnecting;
@property NSMutableDictionary * stateConnected;
@property NSMutableDictionary * currentState;

@end



@implementation KSClientSocketStateMachine

- (id)initWithClientSocket:(KSClientSocket *)clientSocket
{
    self = [super init];
    
    self.clientSocket       = clientSocket;
    self.stateIdle          = [NSMutableDictionary dictionary];
    self.stateConnecting    = [NSMutableDictionary dictionary];
    self.stateConnected     = [NSMutableDictionary dictionary];
    
    [self createStates];
    [self updateState:self.stateIdle];
    
    return self;
}

- (void)createStates
{
    __block __weak KSClientSocketStateMachine *_self = self;
    
    /*-------------------------------------------------------------------------------
     - enter / leave
     -------------------------------------------------------------------------------*/
    [self.stateIdle setKey:@"enter" value:^
    {
        NSLog(@"enter to state idle");
        [_self.clientSocket cleanup];
    }];
    [self.stateConnecting setKey:@"enter" value:^
    {
        NSLog(@"enter to state connecting");
        [_self.clientSocket startConnect];
    }];
    [self.stateConnected setKey:@"enter" value:^
    {
        NSLog(@"enter to state connected");
        [_self.clientSocket startReceive];
    }];


    [self.stateIdle setKey:@"leave" value:^
    {
        NSLog(@"leave from state idle");
    }];
    [self.stateConnecting setKey:@"leave" value:^
    {
        NSLog(@"leave from state connecting");
    }];
    [self.stateConnected setKey:@"leave" value:^
    {
        NSLog(@"leave from state connected");
    }];


    /*-------------------------------------------------------------------------------
     - requests
     -------------------------------------------------------------------------------*/

    //notifyStartRequested
    [self.stateIdle setKey:@"notifyStartRequested" value:^
    {
        [_self updateState:_self.stateConnecting];
    }];
    [self.stateConnecting setKey:@"notifyStartRequested" value:^
    {
        [_self.clientSocket emitError:@"error, already started"];
    }];
    [self.stateConnected setKey:@"notifyStartRequested" value:^
    {
        [_self.clientSocket emitError:@"error, already started"];
    }];


    // notifySendRequested
    [self.stateIdle setKey:@"notifySendRequested" value:^(NSString * message)
    {
        [_self.clientSocket emitError:@"error, not connected yet."];
    }];
    [self.stateConnecting setKey:@"notifySendRequested" value:^(NSString * message)
    {
        [_self.clientSocket emitError:@"error, not connected yet."];
    }];
    [self.stateConnected setKey:@"notifySendRequested" value:^(NSString * message)
    {
        [_self.clientSocket sendMessage:message];
    }];

    
    // notifyCloseRequested
    [self.stateIdle setKey:@"notifyCloseRequested" value:^
    {
        [_self.clientSocket emitError:@"error, not started"];
    }];
    [self.stateConnecting setKey:@"notifyCloseRequested" value:^
    {
        [_self updateState:_self.stateIdle];
    }];
    [self.stateConnected setKey:@"notifyCloseRequested" value:^
    {
        [_self updateState:_self.stateIdle];
    }];

    
    /*-------------------------------------------------------------------------------
     - socket events
     -------------------------------------------------------------------------------*/
    
    // notifyConnected
    [self.stateIdle setKey:@"notifyConnected" value:^
    {
        // nothing to do
    }];
    [self.stateConnecting setKey:@"notifyConnected" value:^
    {
        [_self.clientSocket emitConnected];
        [_self updateState:_self.stateConnected];
    }];
    [self.stateConnected setKey:@"notifyConnected" value:^
    {
        // nothing to do
    }];

    
    // notifyReceived
    [self.stateIdle setKey:@"notifyReceived" value:^
    {
        // nothing to do
    }];
    [self.stateConnecting setKey:@"notifyReceived" value:^
    {
        // nothing to do
    }];
    [self.stateConnected setKey:@"notifyReceived" value:^
    {
        [_self.clientSocket dispatchReceivedMessages];
    }];

    
    // notifyDisconnected
    [self.stateIdle setKey:@"notifyDisconnected" value:^
    {
        // nothing to do
    }];
    [self.stateConnecting setKey:@"notifyDisconnected" value:^
    {
        // nothing to do
    }];
    [self.stateConnected setKey:@"notifyDisconnected" value:^
    {
        [_self.clientSocket emitDisconnected];
        [_self.clientSocket cleanup];
        [_self updateState:_self.stateConnecting];
    }];
}


/*-------------------------------------------------------------------------------
 - notify (user request)
 -------------------------------------------------------------------------------*/
- (void)notifyStartRequested
{
    void (^func)() = [self.currentState objectForKey:@"notifyStartRequested"];
    func();
}

- (void)notifySendRequested:(NSString *)message
{
    void (^func)(NSString *) = [self.currentState objectForKey:@"notifySendRequested"];
    func(message);
}

- (void)notifyCloseRequested
{
    void (^func)() = [self.currentState objectForKey:@"notifyCloseRequested"];
    func();
}


/*-------------------------------------------------------------------------------
 - notify (socket event)
 -------------------------------------------------------------------------------*/
- (void)notifyConnected
{
    void (^func)() = [self.currentState objectForKey:@"notifyConnected"];
    func();
}

- (void)notifyReceived
{
    void (^func)() = [self.currentState objectForKey:@"notifyReceived"];
    func();
}

- (void)notifyDisconnected
{
    void (^func)() = [self.currentState objectForKey:@"notifyDisconnected"];
    func();
}



/*-------------------------------------------------------------------------------
 - state swap
 -------------------------------------------------------------------------------*/

- (void)updateState:(NSMutableDictionary *)newState
{
    if(self.currentState != nil)
    {
        void (^func)() = [self.currentState objectForKey:@"leave"];
        func();
    }
    
    self.currentState = newState;
    
    if(self.currentState != nil)
    {
        void (^func)() = [self.currentState objectForKey:@"enter"];
        func();
    }
}

@end
