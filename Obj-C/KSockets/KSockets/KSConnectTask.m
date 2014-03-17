//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

#import "KSConnectTask.h"
#import "KSPosixAPIWrapper.h"

@interface KSConnectTask ()

@property NSString *    serverAddress;
@property uint          serverPort;

@end

@implementation KSConnectTask

- (id)initWithServerAddress:(NSString *)address port:(uint)port
{
    self = [super init];
    
    self.clientSocket   = 0;
    self.serverAddress  = address;
    self.serverPort     = port;
    self.connected      = [[KSEvent alloc] init];
    
    return self;
}

- (void)main
{
    while(YES)
    {
        if(self.isCancelled == YES)
        {
            NSLog(@"connect operation is cancelled -> exit");
            return;
        }
        
        self.clientSocket = 0;
        self.clientSocket = create_client_socket();
        int ret = connect_to_server(self.clientSocket, [self.serverAddress cStringUsingEncoding:NSASCIIStringEncoding], self.serverPort);
  
        if(self.isCancelled == YES)
        {
            NSLog(@"connect operation is cancelled -> exit");
            return;
        }
        if(ret == 0)
        {
            break;
        }
        
        NSLog(@"connect returned failure. try again after a few sec");
        close_socket(&_clientSocket); 
        sleep(5);
    }
    
    [self.connected dispatchEventWithObject:self waitUntilDone:YES];
}

@end
