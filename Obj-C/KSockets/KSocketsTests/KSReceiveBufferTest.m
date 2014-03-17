//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

#import "KSReceiveBufferTest.h"
#import "KSReceiveBuffer.h"

@interface KSReceiveBufferTest ()
@property KSReceiveBuffer * buffer;
@end

@implementation KSReceiveBufferTest

- (void)setUp
{
    self.buffer = [[KSReceiveBuffer alloc] init];
    [super setUp];
}

- (void)tearDown
{
    [super tearDown];
}

- (void)testLength
{
    [self.buffer clear];
    STAssertEquals(self.buffer.length, 0, @"length must be zero");
    
    [self.buffer pushData:"hoge" length:4];
    STAssertEquals(self.buffer.length, 4, @"length must be 4");
    
    [self.buffer pushData:"hage" length:4];
    STAssertEquals(self.buffer.length, 8, @"length must be 8");
    
    [self.buffer getData:3];
    STAssertEquals(self.buffer.length, 5, @"length must be 5");
    
    [self.buffer pushData:"huge" length:4];
    STAssertEquals(self.buffer.length, 9, @"length must be 9");
}

- (void)testReadPosition
{
    [self.buffer clear];
    STAssertEquals(self.buffer.readPosition, 0, @"readPosition must be zero");
    
    [self.buffer pushData:"hoge" length:4];
    STAssertEquals(self.buffer.readPosition, 0, @"readPosition must be zero");
    
    [self.buffer pushData:"hage" length:4];
    STAssertEquals(self.buffer.readPosition, 0, @"readPosition must be zero");
    
    [self.buffer getData:3];
    STAssertEquals(self.buffer.readPosition, 3, @"readPosition must be 3");
    
    [self.buffer pushData:"huge" length:4];
    STAssertEquals(self.buffer.readPosition, 3, @"readPosition must be 3");
    
    [self.buffer getData:3];
    STAssertEquals(self.buffer.readPosition, 6, @"readPosition must be 6");

    [self.buffer getData:6];
    STAssertEquals(self.buffer.readPosition, 0, @"readPosition must be 0");
}

- (void)testWritePosition
{
    [self.buffer clear];
    STAssertEquals(self.buffer.writePosition, 0, @"writePosition must be zero");
    
    [self.buffer pushData:"hoge" length:4];
    STAssertEquals(self.buffer.writePosition, 4, @"writePosition must be 4");
    
    [self.buffer pushData:"hage" length:4];
    STAssertEquals(self.buffer.writePosition, 8, @"writePosition must be 8");
    
    [self.buffer getData:3];
    STAssertEquals(self.buffer.writePosition, 8, @"writePosition must be 8");
    
    [self.buffer pushData:"huge" length:4];
    STAssertEquals(self.buffer.writePosition, 12, @"writePosition must be 12");

    [self.buffer getData:3];
    STAssertEquals(self.buffer.writePosition, 12, @"writePosition must be 12");

    [self.buffer getData:6];
    STAssertEquals(self.buffer.writePosition, 0, @"writePosition must be 0");
}

- (void)testClear
{
    [self.buffer clear];
    STAssertEquals(self.buffer.length, 0, nil);
    STAssertEquals(self.buffer.readPosition, 0, nil);
    STAssertEquals(self.buffer.writePosition, 0, nil);

    [self.buffer pushData:"hoge" length:4];
    STAssertEquals(self.buffer.length, 4, nil);
    STAssertEquals(self.buffer.readPosition, 0, nil);
    STAssertEquals(self.buffer.writePosition, 4, nil);

    [self.buffer clear];
    STAssertEquals(self.buffer.length, 0, nil);
    STAssertEquals(self.buffer.readPosition, 0, nil);
    STAssertEquals(self.buffer.writePosition, 0, nil);
}

- (void)testPushData
{
    [self.buffer clear];
    STAssertEquals(self.buffer.length, 0, @"length must be zero");
    
    [self.buffer pushData:"hoge" length:4];
    STAssertEquals(self.buffer.length, 4, @"length must be 4");
    
    [self.buffer pushData:"hage" length:4];
    STAssertEquals(self.buffer.length, 8, @"length must be 8");
    
    [self.buffer getData:3];
    STAssertEquals(self.buffer.length, 5, @"length must be 5");

    [self.buffer pushData:"huge" length:4];
    STAssertEquals(self.buffer.length, 9, @"length must be 9");
}

- (void)testGetData
{
    NSData * token;
    
    [self.buffer clear];
    [self.buffer pushData:"123abcdあいう" length:16];

    token = [self.buffer getData:3];
    STAssertEquals(memcmp(token.bytes, "123", 3), 0, nil);

    token = [self.buffer getData:4];
    STAssertEquals(memcmp(token.bytes, "abcd", 4), 0, nil);

    token = [self.buffer getData:9];
    STAssertEquals(memcmp(token.bytes, "あいう", 9), 0, nil);

    [self.buffer pushData:"123abcd" length:7];

    token = [self.buffer getData:3];
    STAssertEquals(memcmp(token.bytes, "123", 3), 0, nil);
    
    [self.buffer pushData:"あいう" length:9];

    token = [self.buffer getData:4];
    STAssertEquals(memcmp(token.bytes, "abcd", 4), 0, nil);
    
    token = [self.buffer getData:9];
    STAssertEquals(memcmp(token.bytes, "あいう", 9), 0, nil);
}

- (void)testReadFirst4BytesAsInt
{
    int value1 = htonl(4);
    int value2 = htonl(32);
    int value3 = htonl(123456);
    
    [self.buffer clear];
    [self.buffer pushData:&value1 length:4];
    [self.buffer pushData:&value2 length:4];
    [self.buffer pushData:&value3 length:4];
    
    STAssertEquals([self.buffer readFirst4BytesAsInt], 4, nil);
    [self.buffer getData:4];
    STAssertEquals([self.buffer readFirst4BytesAsInt], 32, nil);
    [self.buffer getData:4];
    STAssertEquals([self.buffer readFirst4BytesAsInt], 123456, nil);
}

- (void)testIndexOf
{
    [self.buffer clear];
    
    [self.buffer pushData:"1231234あいう" length:16];
    STAssertEquals([self.buffer indexOf:"1" length:1], 0, nil);
    STAssertEquals([self.buffer indexOf:"123" length:3], 0, nil);
    STAssertEquals([self.buffer indexOf:"1234" length:4], 3, nil);
    STAssertEquals([self.buffer indexOf:"あいう" length:9], 7, nil);
    STAssertEquals([self.buffer indexOf:"4あい" length:7], 6, nil);
    STAssertEquals([self.buffer indexOf:"う" length:3], 13, nil);
    STAssertEquals([self.buffer indexOf:"え" length:3], -1, nil);
}


@end
