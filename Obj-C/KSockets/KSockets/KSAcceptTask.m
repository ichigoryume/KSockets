//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import "KSAcceptTask.h"
#import "KSPosixAPIWrapper.h"

@interface KSAcceptTask ()

@property uint  port;
@property int   listeningSocket;
@property BOOL  loop;

@end

@implementation KSAcceptTask

- (id)initWithPort:(uint)port loop:(BOOL)loop
{
    self = [super init];
    
    self.port = port;
    self.loop = loop;

    return self;
}

- (void)start
{
    self.listeningSocket = create_listening_socket(self.port);
    [super start];
}

- (void)stop
{
    [super stop];
    close_socket(&_listeningSocket); // このクラスでcloseするか要検討
}

- (void)dealloc
{
    close_socket(&_listeningSocket); // このクラスでcloseするか要検討
}


// なんかいまいちすっきりしない
- (void)main
{
    NSLog(@"start accept");
    while (YES)
    {
        if(self.isCancelled == YES)
        {
            break;
        }
        
        int acceptedSocket = 0;
        NSString * peerAddress;
        peerAddress = accept_connection_from_peer(self.listeningSocket, &acceptedSocket);

        if([self isCancelled] == YES)
        {
            close_socket(&acceptedSocket);
            NSLog(@"accept operation is cancelled -> exit");
            break;
        }
    
        if(peerAddress == nil)
        {
            NSLog(@"accept returned failure");
            sleep(1);
            continue;
        }

        [self dispatchAcceptedEvent:acceptedSocket peerAddress:peerAddress];
        if(self.loop == NO)
        {
            break;
        }
    }
}

- (void)dispatchAcceptedEvent:(int)socket peerAddress:(NSString *)address
{
    KSAcceptedEventArgs * eventArgs = [[KSAcceptedEventArgs alloc] init];
    eventArgs.peerAddress           = address;
    eventArgs.acceptedSocket        = socket;
    
    [self.accepted dispatchEventWithObject:eventArgs waitUntilDone:YES];
}

@end
