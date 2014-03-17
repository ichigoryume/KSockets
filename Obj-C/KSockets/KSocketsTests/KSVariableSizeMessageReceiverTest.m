//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

#import "KSVariableSizeMessageReceiverTest.h"
#import "KSVariableSizeMessageReceiver.h"

@interface KSVariableSizeMessageReceiverTest ()
@property KSVariableSizeMessageReceiver * receiver;
@end

@implementation KSVariableSizeMessageReceiverTest

static char buffer[256];

- (void)setUp
{
    self.receiver = [[KSVariableSizeMessageReceiver alloc] init];
}

- (void)write2Buffer:(char *)message length:(int)length offset:(int)offset
{
    int header = htonl(length);
    memcpy(buffer + offset, &header, 4);
    memcpy(buffer + offset + 4, message, length);
}

- (void)testPush1Message
{
    NSMutableArray * array;
    [self.receiver clear];
    
    // push 1 message
    [self write2Buffer:"123456" length:6 offset:0];
    array = [self.receiver push:buffer length:10];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123456", nil);
    
    // push 1 message
    [self write2Buffer:"あい" length:6 offset:0];
    array = [self.receiver push:buffer length:10];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"あい", nil);
}

- (void)testPushEmptyMessage
{
    NSMutableArray * array;
    [self.receiver clear];
    
    [self write2Buffer:"" length:0 offset:0];
    array = [self.receiver push:buffer length:4];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"", nil);
}

- (void)testPush2Message
{
    NSMutableArray * array;
    [self.receiver clear];
    
    [self write2Buffer:"123456" length:6 offset:0];
    [self write2Buffer:"abcde" length:5 offset:10];
    array = [self.receiver push:buffer length:19];
    STAssertEquals((int)array.count, 2, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123456", nil);
    STAssertEqualObjects([array objectAtIndex:1], @"abcde", nil);
}

- (void)testPush1MessageDividedInto2Fragments
{
    NSMutableArray * array;
    [self.receiver clear];
    
    [self write2Buffer:"123456" length:6 offset:0];
    
    array = [self.receiver push:buffer length:3];
    STAssertEquals((int)array.count, 0, nil);
    
    array = [self.receiver push:buffer + 3 length:7];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123456", nil);

    array = [self.receiver push:buffer length:7];
    STAssertEquals((int)array.count, 0, nil);
    
    array = [self.receiver push:buffer + 7 length:3];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123456", nil);
}

- (void)testPush3MessageDividedInto4Fragments
{
    NSMutableArray * array;
    [self.receiver clear];
    
    [self write2Buffer:"123456" length:6 offset:0];
    [self write2Buffer:"abcdef" length:6 offset:10];
    [self write2Buffer:"ほげ" length:6 offset:20];
    
    array = [self.receiver push:buffer length:13];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123456", nil);
    
    array = [self.receiver push:buffer + 13 length:1];
    STAssertEquals((int)array.count, 0, nil);
    
    array = [self.receiver push:buffer + 14 length:14];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"abcdef", nil);
    
    array = [self.receiver push:buffer + 28 length:2];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"ほげ", nil);
}

-(void)testClear
{
    NSMutableArray * array;
    
    [self write2Buffer:"123456" length:6 offset:0];

    [self.receiver clear];
    array = [self.receiver push:buffer length:3];
    [self.receiver clear];
    
    [self.receiver clear];
    array = [self.receiver push:buffer length:7];
    [self.receiver clear];

    array = [self.receiver push:buffer length:10];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123456", nil);
}

@end
