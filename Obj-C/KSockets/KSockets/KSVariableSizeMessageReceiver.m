//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

#import "KSVariableSizeMessageReceiver.h"
#import "KSReceiveBuffer.h"

@interface KSVariableSizeMessageReceiver ()
@property KSReceiveBuffer * buffer;
@end

@implementation KSVariableSizeMessageReceiver

- (id)init
{
    self = [super init];
    
    self.buffer = [[KSReceiveBuffer alloc] init];
    
    return self;
}

- (void)clear
{
    [self.buffer clear];
}

- (NSMutableArray *)push:(void *)data length:(int)length NS_RETURNS_RETAINED
{
    [self.buffer pushData:data length:length];

    NSMutableArray * array = [NSMutableArray array];
    while([self nextPayloadIsReceivedCompletely])
    {
        [array addObject:[self getNextPayloadAsString]];
    }
    
    return array;
}

- (BOOL)nextPayloadIsReceivedCompletely
{
    if(self.buffer.length < 4)
    {
        return NO;
    }
    
    int messageSize = [self.buffer readFirst4BytesAsInt];
    if(self.buffer.length < 4 + messageSize)
    {
        return NO;
    }
    
    return YES;
}

- (NSString *)getNextPayloadAsString NS_RETURNS_RETAINED
{
    int messageSize = [self.buffer readFirst4BytesAsInt];
    [self.buffer getData:4]; // skip header (size area)

    NSMutableData * payload = [self.buffer getData:messageSize];
    [payload appendBytes:"\0" length:1];
    
    return [NSString stringWithUTF8String:payload.mutableBytes];
}

@end
