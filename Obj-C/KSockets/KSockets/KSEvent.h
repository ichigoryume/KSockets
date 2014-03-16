//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface KSEvent : NSObject

- (void)addEventListener:(id)instance selector:(SEL)selector; // selector must be "xxxx:(id)object"
- (void)removeEventListener:(id)instance selector:(SEL)selector;
- (void)dispatchEventWithObject:(id)object waitUntilDone:(BOOL)waitUntilDone;

@end
