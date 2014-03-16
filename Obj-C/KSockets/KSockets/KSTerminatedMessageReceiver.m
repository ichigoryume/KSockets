//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import "KSTerminatedMessageReceiver.h"
#import "KSReceiveBuffer.h"

@interface KSTerminatedMessageReceiver ()
@property KSReceiveBuffer * buffer;
@property char * terminator;
@property int terminatorLength;
@end

@implementation KSTerminatedMessageReceiver

- (id)initWithTerminator:(NSString *)terminator
{
    self = [super init];
    
    self.buffer = [[KSReceiveBuffer alloc] init];
    [self createTerminator:terminator];
    
    return self;
}

- (void)createTerminator:(NSString *)terminator
{
    // get c string of terminator
    const char * cString = [terminator cStringUsingEncoding:NSUTF8StringEncoding];
    self.terminatorLength = (int)strlen(cString);

    // create copy
    self.terminator = (char *)malloc(self.terminatorLength);
    memcpy(self.terminator, cString, self.terminatorLength);
}

- (void)clear
{
    [self.buffer clear];
}


- (NSMutableArray *)push:(void *)data length:(int)length NS_RETURNS_RETAINED
{
    [self.buffer pushData:data length:length];
    
    NSMutableArray * array = [NSMutableArray array];
    while(YES)
    {
        int terminatorIndex = [self.buffer indexOf:self.terminator length:self.terminatorLength];
        if(terminatorIndex < 0)
        {
            break;
        }
        [array addObject:[self getNextPayloadAsString:terminatorIndex]];
    }
    
    return array;
}

-(NSString *)getNextPayloadAsString:(int)terminatorIndex NS_RETURNS_RETAINED
{
    NSString * string;
    if(terminatorIndex == 0)
    {
        string = @"";
    }
    else
    {
        NSMutableData * payload = [self.buffer getData:terminatorIndex];
        [payload appendBytes:"\0" length:1];
        string = [NSString stringWithUTF8String:payload.bytes];
    }
    
    [self.buffer getData:self.terminatorLength]; // skip terminator
    return string;
}

@end
