//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

#import "KSIReceiver.h"

@interface KSTerminatedMessageReceiver : NSObject <KSIReceiver> 

- (id)initWithTerminator:(NSString *)terminator;

@end
