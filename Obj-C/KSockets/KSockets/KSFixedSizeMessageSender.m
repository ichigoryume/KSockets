//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import "KSFixedSizeMessageSender.h"
#import "KSPosixAPIWrapper.h"

@interface KSFixedSizeMessageSender ()
@property int payloadSize;
@property NSMutableData * buffer;
@end

@implementation KSFixedSizeMessageSender


- (id)initWithPayloadSize:(int)payloadSize
{
    self = [super init];

    self.payloadSize = payloadSize;
    self.buffer = [NSMutableData dataWithLength:payloadSize];
    
    return self;
}

- (void)sendMessage:(NSString *)message socket:(int)socket
{
    // clear buff. important!!
    [self.buffer resetBytesInRange:NSMakeRange(0, self.payloadSize)];
    
    // create c string of message
    const char * messageCString = [message cStringUsingEncoding:NSUTF8StringEncoding];
    int messageCStringLength = (int)strlen(messageCString);
    
    // copy and send
    [self.buffer replaceBytesInRange:NSMakeRange(0, MIN(messageCStringLength, self.payloadSize)) withBytes:messageCString];
    send_n_byte(socket, self.buffer.mutableBytes, self.payloadSize);
}

@end
