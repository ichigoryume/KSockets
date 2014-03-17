//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

@protocol KSISender <NSObject>

- (void)sendMessage:(NSString *)message socket:(int)socket;

@end
