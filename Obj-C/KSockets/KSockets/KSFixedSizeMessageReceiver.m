//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import "KSFixedSizeMessageReceiver.h"
#import "KSReceiveBuffer.h"

@interface KSFixedSizeMessageReceiver ()
@property KSReceiveBuffer * buffer;
@property int payloadSize;
@end

@implementation KSFixedSizeMessageReceiver

- (id)initWithPayloadSize:(int)payloadSize
{
    self = [super init];
    
    self.payloadSize = payloadSize;
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
    while(self.buffer.length >= self.payloadSize)
    {
        [array addObject:[self getNextPayloadAsString]];
    }
    
    return array;
}

-(NSString *)getNextPayloadAsString NS_RETURNS_RETAINED
{
    NSMutableData * payload = [self.buffer getData:self.payloadSize];
    [self checkNullTermination:payload];
    return [NSString stringWithUTF8String:payload.bytes];
}

-(void)checkNullTermination:(NSMutableData *)payload
{
    // null文字が見つからないなら最後に付け足す
    if(strnlen(payload.mutableBytes, self.payloadSize) == self.payloadSize)
    {
        [payload appendBytes:"\0" length:1];
    }
}

@end
