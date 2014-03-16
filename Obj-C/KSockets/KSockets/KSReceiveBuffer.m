//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import "KSReceiveBuffer.h"

@interface KSReceiveBuffer ()

@property NSMutableData * mutableData;
@property int readPosition;
@property int writePosition;

- (void *)dataOfReadPosition;

@end

@implementation KSReceiveBuffer

- (int)length
{
    return self.writePosition - self.readPosition;
}

- (id) init
{
    self = [super init];

    self.mutableData = [NSMutableData dataWithLength:1024];
    [self clear];
    
    return self;
}

- (void)clear
{
    [self.mutableData resetBytesInRange:NSMakeRange(0, self.writePosition)];
    self.readPosition = 0;
    self.writePosition = 0;
}

- (void)pushData:(void *)data length:(int)length
{
    [self.mutableData replaceBytesInRange:NSMakeRange(self.writePosition, length) withBytes:data];
    self.writePosition += length;
}

- (NSMutableData *)getData:(int)length NS_RETURNS_RETAINED
{
    NSMutableData * result = [NSMutableData dataWithBytes:[self dataOfReadPosition] length:length];
    
    self.readPosition += length;
    if(self.length == 0)
    {
        [self clear];
    }
    
    return result;
}

- (int)readFirst4BytesAsInt
{
    return ntohl(*(int *)[self dataOfReadPosition]);
}

- (int)indexOf:(const char *)value length:(int)length;
{
    for(int i = 0; i <= self.length - length; i++)
    {
        if(memcmp(value, [self dataOfReadPosition] + i, length) == 0)
        {
            return i;
        }
    }
    return -1;
}

- (void *)dataOfReadPosition
{
    return [self.mutableData mutableBytes] + self.readPosition;
}


@end
