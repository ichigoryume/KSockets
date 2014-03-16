//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

@protocol KSISender <NSObject>

- (void)sendMessage:(NSString *)message socket:(int)socket;

@end
