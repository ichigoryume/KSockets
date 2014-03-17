//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

#import "KSTerminatedMessageReceiverTest.h"
#import "KSTerminatedMessageReceiver.h"

@interface KSTerminatedMessageReceiverTest ()
@property KSTerminatedMessageReceiver * receiver;
@end

@implementation KSTerminatedMessageReceiverTest

- (void)setUp
{
    self.receiver = [[KSTerminatedMessageReceiver alloc] initWithTerminator:@"TERM"];
}

- (void)testPush1Message
{
    NSMutableArray * array;
    [self.receiver clear];
    
    // push 1 message
    array = [self.receiver push:"123456TERM" length:10];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123456", nil);
    
    // push 1 message
    array = [self.receiver push:"あいTERM" length:10];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"あい", nil);
}

- (void)testPushEmptyMessage
{
    NSMutableArray * array;
    [self.receiver clear];
    
    array = [self.receiver push:"TERM" length:4];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"", nil);
}

- (void)testPush2Message
{
    NSMutableArray * array;
    [self.receiver clear];
    
    array = [self.receiver push:"123456TERMabcdefTERM" length:20];
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
    
    array = [self.receiver push:"456TERM" length:7];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123456", nil);
}

- (void)testPush3MessageDividedInto4Fragments
{
    NSMutableArray * array;
    [self.receiver clear];
    
    array = [self.receiver push:"123456TERMabc" length:13];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123456", nil);
    
    array = [self.receiver push:"d" length:1];
    STAssertEquals((int)array.count, 0, nil);
    
    array = [self.receiver push:"efTERM123456TERMabc" length:19];
    STAssertEquals((int)array.count, 2, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"abcdef", nil);
    STAssertEqualObjects([array objectAtIndex:1], @"123456", nil);
    
    array = [self.receiver push:"defTERM" length:7];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"abcdef", nil);
}

-(void)testClear
{
    NSMutableArray * array;
    
    [self.receiver clear];
    array = [self.receiver push:"abc" length:3];
    [self.receiver clear];
    
    array = [self.receiver push:"123456TERM" length:10];
    STAssertEquals((int)array.count, 1, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123456", nil);
}

- (void)testTerm
{
    KSTerminatedMessageReceiver * receiver;
    NSMutableArray * array;
    
    receiver = [[KSTerminatedMessageReceiver alloc] initWithTerminator:@"\n"];
    array = [receiver push:"123\nabc\n日本語\n" length:18];
    STAssertEquals((int)array.count, 3, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123", nil);
    STAssertEqualObjects([array objectAtIndex:1], @"abc", nil);
    STAssertEqualObjects([array objectAtIndex:2], @"日本語", nil);

    receiver = [[KSTerminatedMessageReceiver alloc] initWithTerminator:@"終端"];
    array = [receiver push:"123終端abc終端日本語終端" length:33];
    STAssertEquals((int)array.count, 3, nil);
    STAssertEqualObjects([array objectAtIndex:0], @"123", nil);
    STAssertEqualObjects([array objectAtIndex:1], @"abc", nil);
    STAssertEqualObjects([array objectAtIndex:2], @"日本語", nil);
}

@end
