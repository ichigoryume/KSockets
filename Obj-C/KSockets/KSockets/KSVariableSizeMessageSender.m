//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import "KSVariableSizeMessageSender.h"
#import "KSPosixAPIWrapper.h"
#import <arpa/inet.h>

@implementation KSVariableSizeMessageSender 

- (void)sendMessage:(NSString *)message socket:(int)socket
{
    // create C string
    const char * body = [message cStringUsingEncoding:NSUTF8StringEncoding];
    
    // create header
    uint bodySize = (uint)strlen(body);
    uint header = htonl(bodySize);
    
    // create payload
    NSMutableData * payload = [NSMutableData dataWithCapacity:(4 + bodySize)];
    [payload appendBytes:(void *)&header length:4];
    [payload appendBytes:body length:bodySize];

    send_n_byte(socket, payload.mutableBytes, 4 + bodySize);
}

@end
