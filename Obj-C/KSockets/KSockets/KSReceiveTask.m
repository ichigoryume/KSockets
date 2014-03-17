//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

#import "KSReceiveTask.h"
#import "KSPosixAPIWrapper.h"

@interface KSReceiveTask ()
@property int socket;
@end

@implementation KSReceiveTask

static int BufferSize = 1024;

- (id)initWithSocket:(int)socket
{
    self = [super init];
    
    self.socket         = socket;
    self.received       = [[KSEvent alloc] init];
    self.socketClosed   = [[KSEvent alloc] init];
    self.buffer         = (char *)malloc(BufferSize);
    
    return self;
}

- (void)dealloc
{
    free(self.buffer);
}

- (void)main
{
    if(self.isCancelled == YES)
    {
        return;
    }
    
    while (YES)
    {
        self.receivedSize = receive_n_byte_at_most(self.socket, self.buffer, BufferSize);
        
        // check disconnected or error
        if([self isCancelled] == YES)
        {
            NSLog(@"receive operation is cancelled -> exit");
            break;
        }
        
        if(self.receivedSize <= 0)
        {
            NSLog(@"socket is disconnected or broken");
            [self.socketClosed dispatchEventWithObject:self waitUntilDone:YES];
            break;
        }
        
        [self.received dispatchEventWithObject:self waitUntilDone:YES];
    }
    
}

@end
