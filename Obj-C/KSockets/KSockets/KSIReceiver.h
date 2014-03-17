//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

@protocol KSIReceiver <NSObject>

- (void)clear;

// 受信した生データを入れると、受信メッセージの配列を（あれば）返す
- (NSMutableArray *)push:(void *)data length:(int)length NS_RETURNS_RETAINED;

@end
