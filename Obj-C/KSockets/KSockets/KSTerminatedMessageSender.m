//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import "KSTerminatedMessageSender.h"
#import "KSPosixAPIWrapper.h"

@interface KSTerminatedMessageSender ()
@property char *  terminator;
@property int     terminatorLength;
@end

@implementation KSTerminatedMessageSender

- (id)initWithTerminator:(NSString *)terminator
{
    self = [super init];
    
    // create C string of terminator
    const char * terminatorWithNull = [terminator cStringUsingEncoding:NSUTF8StringEncoding];
    self.terminatorLength = (int)strlen(terminatorWithNull);
    
    // copy excepting ¥0
    self.terminator = (char *)malloc(self.terminatorLength);
    memcpy(self.terminator, terminatorWithNull, self.terminatorLength);
    
    return self;
}

- (void)sendMessage:(NSString *)message socket:(int)socket
{
    // create C string of body
    const char * body = [message cStringUsingEncoding:NSUTF8StringEncoding];
    int bodyLength = (int)strlen(body);
    
    // create payload
    NSMutableData * payload = [NSMutableData dataWithCapacity:(bodyLength + self.terminatorLength)];
    [payload appendBytes:body length:bodyLength];
    [payload appendBytes:self.terminator length:self.terminatorLength];
    
    // send
    send_n_byte(socket, payload.mutableBytes, bodyLength + self.terminatorLength);
}

@end
