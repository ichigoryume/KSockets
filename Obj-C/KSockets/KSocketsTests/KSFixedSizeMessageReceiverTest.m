//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

#import "KSFixedSizeMessageReceiverTest.h"
#import "KSFixedSizeMessageReceiver.h"

@interface KSFixedSizeMessageReceiverTest ()
@property KSFixedSizeMessageReceiver * receiver;
@end

@implementation KSFixedSizeMessageReceiverTest

- (void)setUp
{
    self.receiver = [[KSFixedSizeMessageReceiver alloc] initWithPayloadSize:6];
}

- (void)testPush1Message
{
    NSMutableArray * array;
    [self.receiver clear];
    
    // push 1 message
    array = [self.receiver push:"123456" length:6];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123456", nil);
    
    // push 1 message
    array = [self.receiver push:"あい" length:6];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"あい", nil);
}

- (void)testPushEmptyMessage
{
    NSMutableArray * array;
    [self.receiver clear];
    
    array = [self.receiver push:"\0\0\0\0\0\0" length:6];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"", nil);
}

- (void)testPush2Message
{
    NSMutableArray * array;
    [self.receiver clear];
    
    array = [self.receiver push:"123456abcdef" length:12];
    STAssertEquals((int)array.count, 2, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123456", nil);
    STAssertEqualObjects([array objectAtIndex:1], @"abcdef", nil);
}

- (void)testPush1MessageDividedInto2Fragments
{
    NSMutableArray * array;
    [self.receiver clear];
    
    array = [self.receiver push:"123" length:3];
    STAssertEquals((int)array.count, 0, nil);

    array = [self.receiver push:"456" length:3];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123456", nil);
}

- (void)testPush3MessageDividedInto4Fragments
{
    NSMutableArray * array;
    [self.receiver clear];
    
    array = [self.receiver push:"123456abc" length:9];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123456", nil);

    array = [self.receiver push:"d" length:1];
    STAssertEquals((int)array.count, 0, nil);
    
    array = [self.receiver push:"ef123456abc" length:11];
    STAssertEquals((int)array.count, 2, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"abcdef", nil);
    STAssertEqualObjects([array objectAtIndex:1], @"123456", nil);
    
    array = [self.receiver push:"def" length:3];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"abcdef", nil);
}

-(void)testClear
{
    NSMutableArray * array;

    [self.receiver clear];
    array = [self.receiver push:"abc" length:3];
    [self.receiver clear];
    
    array = [self.receiver push:"123456" length:6];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123456", nil);
}

@end
